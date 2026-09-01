# 5. Transform the event store to Lambert 2008

Date: 2026-09-01

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-projections.md) made every reader of a parcel geometry cope with either
reference system. [ADR 0004](0004-lambert2008-consumer-address.md) did the same for the consumed address
positions, which follow address-registry's conversion rather than this one. Both left the transformation
of the parcel event store itself open, and ADR 0003 explicitly deferred the write side —
`Parcel.GuardPolygon`, `GmlHelpers.GmlToExtendedWkbGeometry` and `ExtendedWkbGeometry.SridLambert72` —
along with it.

This ADR covers the transformation: the domain change that expresses it, what each projection does with
it, and the one-shot job that drives it. It mirrors what address-registry decided in its ADR 0005, in
[address-registry#1379](https://github.com/Informatievlaanderen/address-registry/pull/1379).

## Decision

### One command per stream, one event per parcel

`TransformToLambert2008` takes a `ParcelId` and nothing else — the transformation has nothing to decide
per parcel. A parcel stream holds exactly one geometry, so unlike address-registry, where one command
covers a street name's ~10 addresses, here the number of commands, events and streams are all the same
number (~10^6).

It applies `ParcelGeometryCrsWasChanged`. That name is not an invention: the contract
`Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelGeometryCrsWasChanged` already exists in the version of
GrAr.Contracts this repository references, so the Kafka message and its shape were already settled
elsewhere and the domain event mirrors it field for field — including `CaPaKey`, which the transformation
does not change.

Restating `CaPaKey` is the decision that made everything downstream cheap. Every existing
`ParcelGeometryWasChanged` handler touches only fields the two events share, so each new projection
handler is a copy of its `ParcelGeometryWasChanged` counterpart rather than a hand-written variant. A
geometry-only event was considered and rejected for exactly that reason.

### The aggregate method is deliberately unguarded

`Parcel.TransformToLambert2008()` has no removal guard and no status guard, unlike `ChangeGeometry`. It is
not an edit of the parcel but a change of the reference system its geometry is expressed in, and it has to
reach every parcel the event store holds — removed and retired ones included — or the event store would be
left holding both systems indefinitely.

It also does not run `GuardPolygon`. That guard requires SRID 31370, which is precisely what the
transformation leaves behind; running it would reject every geometry the job is there to convert.

A parcel whose geometry is already Lambert 2008 applies nothing. That is what makes re-running the
transformation over a stream a no-op rather than a double transform, and it is what the migrator's
restart-heavy operating model depends on.

### `TransformFromLambert72To08`, not `EnsureLambert08`

`EnsureLambert08` only transforms geometries that actually fall inside Flanders and *relabels* everything
else (see ADR 0003). For a projection that is harmless. For the event store it would silently corrupt any
geometry outside the envelope — writing Lambert 72 coordinates under SRID 3812, ~500 km from where the
parcel is. The transformation therefore uses `LambertTransformation.TransformFromLambert72To08` with
`roundingPrecision: 2`, which transforms unconditionally, at the centimetre precision geometries are
persisted at.

### Reading the current geometry

`ParcelRegistry.WKBReaderFactory.CreateForEwkb` falls back to the Lambert 72 reader for bytes that carry
no SRID, which is what geometries written before the event store wrote EWKB look like. Those are Lambert
72 by definition, so they transform like any other and come out carrying SRID 3812 — the transformation
also fixes the missing SRID.

There is a trap worth recording. `Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology` declares a
`WKBReaderFactory` of its own, and the using directive for it in `Parcel.cs` — needed there for
`CreateForLambert72()`, which `ParcelRegistry.WKBReaderFactory` does not have — outranks
`ParcelRegistry.WKBReaderFactory` from the enclosing namespace. An unqualified `CreateForEwkb` call binds
to GrAr's version, which *throws* on SRID-less EWKB instead of falling back, and it would compile and pass
every test that used a normal EWKB geometry. `Parcel.TransformToLambert2008()` therefore qualifies the
call. Address-registry hit the same trap and solved it with a file-level alias; that is not available here
because the same file needs both factories.

### What each projection does

The rule is that the transformation does not change the parcel. It changes the units its geometry is
expressed in, so the stored geometry follows the event store, but nothing that means "this object changed"
reacts.

| Projection | Geometry | Version |
|---|---|---|
| Legacy detail | Gml updated, re-labelled EPSG 3812 | **not** bumped |
| Legacy syndication | — | `DoNothing` |
| Integration latest item v2 | updated | **not** bumped |
| Integration version | new version row | — |
| Feed | document updated | no cloud event, `LastChangedOn` untouched |
| Extract (parcel, link) | holds none | **not** bumped |
| WFS | holds none | **not** bumped |
| BackOffice | — | `DoNothing` |
| LastChangedList (v1, v3) | — | bumped |
| Producers (migrate, Oslo snapshot) | — | produced |

Parcel-registry has far less to do here than address-registry, for the same reason ADR 0003 gave: most
projections never touch the geometry. Four points need their reasoning written down.

**Two projections hold no geometry at all.** `ParcelExtractProjections` writes only a dbase record, and
`ParcelWfsItem` holds only the parcel's attributes; both react to `ParcelGeometryWasChanged` purely by
bumping their version. For the transformation there is nothing to reproject *and* no version to bump, so
both are `DoNothing`. That is the whole of their change.

**`LastEventHash` is still updated** on the legacy detail projection, even though the version is not. It is
not a version: Api.Oslo serves it as the ETag and the BackOffice checks the caller's ETag against the
*aggregate's* `LastEventHash`, which the transformation event does change. Freezing the projection's copy
would make the first edit of every parcel after the transformation fail with a 412.

**The feed updates the document but produces no cloud event.** Consumers are not told the parcel changed,
because it did not — but the document has to follow the event store, or the feed would keep serving the
pre-transformation geometry on every subsequent event. Note that
`context.Entry(document).Property(x => x.Document).IsModified = true` lives inside `AddCloudEvent`: the
`Document` column is not change-tracked, so a handler that skips the cloud event has to mark it itself or
the write is silently dropped.

**Legacy syndication does nothing**, pending a decision with the analysts. The consequence is that the
syndication item keeps its Lambert 72 geometry until the parcel's next real change. Output stays correct
either way, because the `objectCrs` filter reprojects on read (ADR 0003), but the syndication table and the
detail table will disagree on SRID after the transformation. If that is not wanted, the fix is the same
shape as the feed's: update the row in place instead of cloning a new version.

**No projection needs a rebuild.** Every one of them handles the event, so each converges on its own as
the transformation runs. That is a property worth keeping rather than a coincidence.

### The migrator

`ParcelRegistry.Migrator.Lambert2008` is a console application shaped after
`AddressRegistry.Migrator.Lambert2008` and matching `ParcelRegistry.Snapshot.Verifier`'s hosting. It pages
`[ParcelRegistry].[Streams]` filtered to `parcel-%`, loads each aggregate, and dispatches the command. The
legacy parcel streams and the all-stream carry bare GUIDs as stream ids, so that filter is exactly the
Parcel aggregate's streams.

Its operating model is *stop and evaluate*, not one long run, which drives most of its design:

- **`MaxPagesPerRun`** lets a run do a bounded amount of work and exit cleanly, rather than being killed
  mid-page.
- **`[ParcelRegistryLambert2008Migration].[ProcessedStreams]`** records one row per stream with `ParcelId`,
  `GeometryByteCount`, `WasConverted`, `LoadMilliseconds` and `DispatchMilliseconds`. Timings are persisted
  rather than only logged so a test run can be *queried* afterwards — cost against geometry size, the
  slowest streams, percentiles — instead of reconstructed from log lines. Load and dispatch are measured
  separately because they scale with different things (stream length versus the size of the geometry
  written) and which dominates is the thing a test run exists to find out. The geometry size is recorded
  because polygon complexity, not address count, is what makes one parcel cost more than another here.
- **`IsPageCompleted`** makes the resume cursor a watermark rather than a guess. Streams within a page are
  processed in parallel, so a recorded high id says nothing about the ids below it; a completed page does.
- **The bookkeeping insert is not cancellable.** It records work the event store has already accepted, and
  losing the row on a Ctrl-C would leave a transformed stream looking untransformed.
- **`DryRun` defaults to `true`**, so the job cannot transform by accident. A dry run loads and measures
  every stream and reports how many parcels would be transformed, but dispatch timings are not recorded at
  all rather than recorded as zero.

Idempotency at the aggregate covers what the bookkeeping cannot: a stream dispatched but not recorded is
re-dispatched on the next run and applies nothing.

The parcel factory takes an `IAddresses`, which only the attach and detach paths ever read. The migrator
registers an implementation that throws instead of wiring up `Consumer.Address`, so a job that has no
business reading that database cannot start depending on it unnoticed.

### Deployment

On a test environment, set `DistributedLock:Enabled` to `false`. The lease is five minutes with
`TerminateApplicationOnFailedRenew`, so killing the container mid-page can block the next start until it
expires — which is exactly what the stop-and-evaluate loop does repeatedly.

## Consequences

- **The write side is still Lambert 72 and has to follow.** `GmlHelpers.GmlToExtendedWkbGeometry` stamps
  SRID 31370 and `Parcel.GuardPolygon` rejects anything else, so the first real geometry change after the
  transformation — a GRB import or a BackOffice edit — writes that parcel back to Lambert 72. Left alone,
  the transformation would be undone parcel by parcel. The cutover is therefore: freeze editing, run the
  migrator to completion, release the write-side change, unfreeze. Until the write side lands, a full run
  only makes sense on a test environment.
- **Every projection holds a mix of reference systems while the transformation runs.** ADR 0003 covers what
  that means per projection; the reason to run this as one pass rather than trickling it is that the mixed
  window is visible to anything doing a spatial query across the two.
- Kafka consumers receive a `ParcelGeometryCrsWasChanged` message per parcel. The feed does not carry one,
  so a consumer reading the feed rather than Kafka sees the new coordinates only on the parcel's next real
  change.
- The syndication feed's stored geometry stays Lambert 72 until the open question above is settled.
- Versions and `VersionTimestamp`s do not move for ~10^6 parcels, so anything downstream that polls "what
  changed since" will not see the transformation. That is the intent, and it is the reason LastChangedList
  is the one exception.
- Re-running the migrator over an already-transformed store is safe and cheap: every stream loads, nothing
  applies, and the bookkeeping table tells you it is done.
