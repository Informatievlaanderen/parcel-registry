# 4. Hold consumed address positions in both reference systems

Date: 2026-08-26

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-projections.md) covered the read side of `Projections.Legacy`,
`Projections.Integration` and the syndication feed, and explicitly deferred this one: `Consumer.Address`
"reads *address* positions off Kafka with a reader pinned to Lambert 72 and is therefore driven by
address-registry's conversion, not this one".

That is the distinguishing fact. Every other consumer in this repository follows the *parcel* event store
and converts when it converts. `Consumer.Address` follows the *address* event store, and the two are
converted on different dates.

### What the table is for

`[ParcelRegistryConsumerAddress].[Addresses]` is not a projection anyone reads. It exists to answer one
question, from one caller:

- `ConsumerAddressContext.FindAddressesWithinGeometry` — which addresses fall inside a parcel polygon —
  called by `ImportParcelHandler` and `ChangeParcelGeometryHandler` in the GRB importer.
- `ConsumerAddressContext.GetOptional` also reads the table, but only for status and removal. It never
  touches `Position`.

Nothing outside this repository reads it, and no API response is derived from it. So unlike
`Projections.Integration`, this table owes no consumer a faithful copy of what the event store holds. It
owes the importer a spatial index that answers point-in-polygon correctly. That freedom is what this ADR
spends.

### The conversion sequence

The design depends on the operational sequence, so it is recorded here as a premise rather than left
implicit. If the sequence changes, revisit this ADR.

It has changed once already. The table below is the revised one:
[ADR 0005](0005-lambert2008-event-store-transformation.md) stopped the importer normalizing the polygon it
hands to the address lookup, which decoupled T2 from the parcel conversion entirely. The original table had
the parcel conversion sitting between T1 and T2 and the importer switching to the Lambert 2008 column as
soon as it came back up; neither is true any more.

| | Event | `Addresses` table | GRB importer |
|---|---|---|---|
| T0 | This change is deployed | 72 complete, 08 empty | running, querying 72 |
| T0→T1 | Global editing freeze. The address, parcel and building event stores are converted **concurrently**; this consumer is paused across the parcel migrator's run to keep its load off parcel's database | filled when the consumer is resumed and drained, still inside the freeze | **paused by the freeze** |
| T1 | Freeze lifted | both complete | running, querying 72 |
| T1→T2 | Normal operation, for as long as is wanted | both complete | running, querying 72 |
| T2 | GRB reader switched to Lambert 2008 GML | both complete | running, querying 08 |
| T3 | Lambert 72 column dropped | 08 only | running, querying 08 |

The last column says which column `FindAddressesWithinGeometry` reads, and that is decided by the
coordinates of the polygon GRB delivered — not by what the parcel event store holds. **The parcel event
store is Lambert 2008 from T1 onwards while the importer goes on querying the Lambert 72 address column
until T2.** That separation is what makes T2 a date someone picks rather than a consequence of the parcel
conversion, and it is the whole reason the address and parcel conversions can overlap.

If the consumer is left running through the freeze rather than paused, the only difference is that the
Lambert 2008 column fills during T0→T1 instead of at the end of it. It is complete before T1 either way,
which is all anything downstream depends on.

Two properties of this sequence are load-bearing:

- **Every address is converted, and each conversion is an event.** The address conversion is a full
  convert including removed addresses, emitting `AddressPositionCrsWasChanged` per address. That is a
  complete rewrite of this table, delivered for free, in seconds of consumer lag. It happens once.
- **T2 is chosen, not caused.** Nothing in the address or parcel conversions moves it, so no Lambert 2008
  parcel polygon reaches the address lookup until someone switches the GRB reader — by which point the
  Lambert 2008 column has been complete since T1. `GuardLambert2008PositionsAreComplete` enforces that
  rather than trusting it, because the failure it prevents is silent.

### Three constraints on any design

1. **SQL Server returns `NULL`, not an error, on an SRID mismatch.** `STContains` and `STTouches` between
   a 31370 polygon and a 3812 point yield no match. A column holding both reference systems means parcels
   import with *zero* addresses attached — no exception, no log line, no failed run. This is the opposite
   of PostGIS, which raises `ERROR: Operation on mixed SRID geometries`, and it is why the mixed column
   ADR 0003 accepted for `Projections.Integration` is not acceptable here.
2. **The spatial index bounding box is in Lambert 72 coordinates.** `SPATIAL_Addresses_Position` is
   declared `BOUNDING_BOX = (22279.17, 153050.23, 258873.3, 244022.31)`. Lambert 2008 Flanders is
   approximately (521398, 652516)–(759275, 744502), entirely outside it, so Lambert 2008 rows fall outside
   the tessellated space and the index stops filtering them.
3. **The two conversions are days apart.** Any design in which this table's reference system is coupled to
   the parcel side's is wrong for the duration.

## Decision

### Two columns, each pinned to one reference system

`AddressConsumerItem` gains `PositionLambert2008` alongside `Position`, each with its own spatial index.
Both are written on every position-bearing event; the query picks the one matching the parcel polygon.

The alternative designs are set out under "Considered and rejected" below. This one is chosen for three
reasons, in order of weight:

- **The importer never pauses for the address conversion.** The Lambert 72 column stays complete and
  correctly indexed from T0 to T3, and the conversion event does not touch it, so nothing this design does
  would ever stop the importer. It is paused across T0→T1 by the editing freeze, which is there for the
  event stores and would be there whatever this table looked like; a design that pinned the column to one
  reference system would have needed a pause of its own, on top.
- **The Lambert 2008 column populates itself.** The conversion events fill it. No backfill job, no
  truncate-and-replay, no offset override. SQL Server has no reprojection function, so a backfill would
  have to run in application code over every row — the free rebuild is worth catching, and it only
  happens once.
- **There is a way back.** Through the whole conversion the Lambert 72 column is intact and current. If
  something is wrong at T2, the fix is to query the other column.

The cost is a second spatial index and a nullable column carried from T0 to T3, and a third deploy to
remove them.

### Positions are read in the reference system they were persisted in

`BackOfficeKafkaProjection` builds one `WKBReader` in its constructor, pinned to
`ExtendedWkbGeometry.SridLambert72`, and parses every position through it. `WKBReader` takes the SRID from
the bytes, so this already reads Lambert 2008 correctly — the same accident of the floating precision
model ADR 0003 declined to rely on. The cached reader is dropped for
`ParcelRegistry.WKBReaderFactory.CreateForEwkb(bytes)` per position.

It must be **`ParcelRegistry.WKBReaderFactory`**, not `GrAr.Common`'s: addresses migrated before the
address event store wrote EWKB carry no SRID, and GrAr's factory throws `ArgumentException("No SrID found
in EWKB")` on those. The wrapper falls back to the Lambert 72 reader, which is the same assumption
`ExtendedWkbGeometry.CreateEWkb` makes. ADR 0003 records the namespace-resolution trap that makes these
two types easy to confuse silently; `BackofficeKafkaProjection.cs` carries an explicit
`using WKBReaderFactory = ParcelRegistry.WKBReaderFactory;` alias for the same reason
`ParcelDetailProjections` does.

### Both columns are written from one parsed geometry

```csharp
var point = (Point)WKBReaderFactory.CreateForEwkb(bytes).Read(bytes);

Position            = point.IsLambert72() ? point : point.EnsureLambert72().RoundCoordinates(2);
PositionLambert2008 = point.IsLambert08() ? point : point.EnsureLambert08(2);
```

Whichever system the event carries passes through untransformed; the other is derived. Rounding is applied
only on the transformed path, to 2 decimals — the centimetre precision positions are persisted at and the
transform is accurate to — matching what `ParcelSyndicationResponse.ToRequestedCrs` and address-registry's
Oslo version 2 do. A position that needs no transform is not rounded, so it stays byte-identical to what
the event store holds.

Before T1 this means today's behaviour on `Position` exactly, plus a derived Lambert 2008 value. After T1
it means the reverse. **`Position` becomes a derived column at T1** for every address that moves after it,
and is no longer byte-identical to the event store for those; that is the deliberate trade, and it is why
the table is described above as owing the importer an index rather than owing anyone a copy.

### …except on the conversion event, which does not write `Position`

The rule above applies to every position-bearing event *except* `AddressPositionCrsWasChanged`, which
writes `PositionLambert2008` only and leaves `Position` alone.

That event does not move the address; it re-expresses it. Transforming its Lambert 2008 payload back to
Lambert 72 would replace an exact, as-published coordinate with a centimetre-rounded round trip of itself.
Positions are already persisted at centimetre precision, so most rows would round-trip to the identical
value — but not all, and which ones cannot be predicted.

Applying that drift is not free, because it would be applied to **every address in the register at once,
during the window in which the importer is still querying `Position`** — T1 precedes T2. The consequence
is specific: `FindAddressesWithinGeometry` has a `Touches` branch, which exists because addresses sitting
exactly on a parcel boundary are real in this data. Move one by a centimetre and it is neither contained
nor touching, and the next `ChangeParcelGeometry` detaches it.

Every *other* position event after T1 does write both columns, because those are genuine moves and
`Position` is queried until T2. Letting them go stale would put the address where it used to be.

`Position` therefore ends up holding original values for addresses that were only ever converted, and
derived ones for addresses that actually moved after T1 — a derived value appears only where there is no
original left to preserve.

This rests on the address conversion being a pure reprojection, which is address-registry's process rather
than ours. It is taken on trust rather than verified per event.

`GivenAddressPositionInEitherReferenceSystem.WhenCrsWasChanged_ThenLambert72PositionIsLeftUntouched` pins
this. It uses a position carrying more precision than a transformed one is rounded to — positions like
`198794.27000000083` are real — so writing `Position` on this event fails the test with
`140000.12` against `140000.123456`, rather than passing because a near-integer coordinate happened to
round-trip onto itself.

### `AddressPositionCrsWasChanged` is load-bearing

The event updates geometry method, specification and `PositionLambert2008` — but not `Position`, for the
reason given above. Under a design that pinned the table to one reference system this event would be
cosmetic. Here it is the mechanism that fills `PositionLambert2008` for the entire table. **Not handling
it means no free rebuild and a backfill instead.**

The contract has been in `Be.Vlaanderen.Basisregisters.GrAr.Contracts` since 24.4.0 and this repository
pins 26.0.0, so it can be handled before address-registry produces it.

`CommandHandlingKafkaProjection` is deliberately not touched. A CRS change does not move an address, so
nothing attaches, detaches or is readdressed by it.

### The query dispatches on the envelope, not the SRID

`FindAddressesWithinGeometry` picks its column with `IsInsideFlandersUsingLambert08()`, after
`GeometryFixer.Fix`, and not by inspecting `geometry.SRID`.

**The parcel polygon's SRID cannot be trusted at this call site.** `GrbXmlReader` reads GRB GML through
`GmlHelpers.CreateGmlReader()`, which is built on the Lambert 72 geometry factory, so every polygon
reaching this method carries SRID 31370 by construction — including, once GRB delivers Lambert 2008, ones
whose coordinates are nothing of the sort. Dispatching on the SRID would query the Lambert 72 column with
Lambert 2008 coordinates and return nothing, for every parcel, silently.

Once the importer's write side is fixed the SRID will be trustworthy, and the envelope check will agree
with it. It costs one comparison and cannot drift out of sync with what the coordinates actually are, so
it is kept regardless. The two envelopes do not overlap — Lambert 72 Flanders spans x 21492–259366,
Lambert 2008 spans x 521398–759275 — so the dispatch is unambiguous.

`GeometryFixer.Fix` stays where it is. It predates this change and is there because SQL Server's
predicates need a valid polygon.

### Two guards, because both failures are silent

- **A Lambert 2008 query while any non-removed row still has a NULL `PositionLambert2008` fails loudly.**
  The sequence above makes this unreachable — T2 follows T1 — so this guard exists to convert an
  assumption about someone else's conversion into a stopped importer rather than a set of parcels that
  quietly lost their addresses.
- **A position outside both Flanders envelopes fails loudly on the write path.**
  `LambertTransformation` decides by envelope, so a point outside both boxes is not transformed at all —
  it just has an SRID stamped on unmoved coordinates, putting it ~500 km from where it belongs. The
  address then falls inside no parcel. The consumer is replayable, so stopping is cheap and a wrong
  position is not.

### Migration

`20260826122429_AddPositionLambert2008` adds `PositionLambert2008` as a nullable `sys.geometry` column and
its spatial index:

```
BOUNDING_BOX = (522200, 653000, 758900, 744100)
```

These are the existing Lambert 72 bounding box's four corners transformed and the envelope padded out to
the next 100 m. The same numbers were derived for address-registry's `AddressWfsV3` spatial index; they
are reused rather than re-derived. The grid and `CELLS_PER_OBJECT` settings are copied from the Lambert 72
index unchanged.

The column is nullable because it is empty at T0 and fills over T0→T1. SQL Server does not index NULLs, so
the index is cheap until the conversion reaches it. `Position` and its index are untouched.

The existing clustered primary key on `AddressPersistentLocalId` satisfies the spatial index requirement.

### End state

At T3, once all parcels are on Lambert 2008 and the importer is confirmed querying the Lambert 2008
column, a second migration drops `Position` and `SPATIAL_Addresses_Position`, the dispatch collapses to a
single column, and the Lambert 72 write path goes with it.

Whether `PositionLambert2008` is then renamed to `Position` is left open. It costs a further migration and
buys a better name; it is called out here so the choice is made rather than defaulted into.

## Considered and rejected

### One column pinned to Lambert 72, transforming the parcel polygon on read

`Position` stays Lambert 72 forever; every incoming position is normalised with `EnsureLambert72()` and
the parcel polygon is transformed to Lambert 72 at query time. No migration, no second index, no NULL
window, and — its real merit — no dependence on anything outside this repository: it is correct whatever
order the two conversions happen in and whether or not every address gets a conversion event.

Rejected because it forfeits the free rebuild. Moving the table to Lambert 2008 afterwards would then cost
a backfill in application code or a truncate-and-replay with the importer paused, and the stated intent is
to be on Lambert 2008 as soon as the address conversion completes. It would also leave every stored
position permanently derived, where the chosen design ends at T3 holding positions exactly as the event
store wrote them.

### One column flipped to Lambert 2008 at deploy, pausing the importer across the address conversion

Strictly the simplest end state: one column, one index, one migration now and none later, no NULL window,
no dispatch. Viable only because the importer can be paused — between T0 and T1 the single column is
genuinely mixed, and by constraint 1 a Lambert 72 polygon queried against it silently misses every
already-converted address.

Rejected on three counts. It imposes importer downtime across the whole address conversion that the
parcel-conversion pause does not already require. It has no rollback: once the conversion has rewritten
rows to Lambert 2008 there is no route back to Lambert 72 without a replay. And it couples this deploy to
the conversion's start date, so a slip in the conversion keeps the importer down.

### One column, allowed to hold both

What ADR 0003 accepted for `Projections.Integration`. Rejected here by constraints 1 and 2 together: SQL
Server's silent `NULL` turns a mismatch into an empty result set rather than an error, and the spatial
index's bounding box does not cover Lambert 2008 at all. The property it buys — the table mirroring the
event store — has no reader in this repository that wants it.

## Consequences

- While the address event store holds Lambert 72, `Position` is byte-for-byte what it is today. All new
  behaviour is on the Lambert 2008 path, which no production data reaches until T1, so this change is
  independent of the conversion's timing.
- `Position` holds its original, as-published Lambert 72 value for any address that is only converted, and
  a derived, centimetre-rounded one for any address that moves after T1. Nothing outside this repository
  reads it either way. It is deleted at T3.
- The `Addresses` table carries two spatial indexes between T0 and T3. Both are maintained only when their
  column changes, so status-only address events cost nothing extra. The peak is the address conversion
  itself, which is a position change on every row and therefore rebuilds both.
- Between T0 and T1, `PositionLambert2008` is NULL for every address not touched since T0. This is safe
  because nothing queries that column: the importer is paused by the freeze for most of the window, and
  what it queries when it comes back is decided by GRB's coordinates, which are Lambert 72 until T2. The
  parcel event store converting inside this window does not change that. The guard above is what makes
  "nothing queries it" enforced rather than assumed.
- The consumer transforms one position per position-bearing event, in one direction or the other, for the
  whole life of the two columns. It is a point transform, some microseconds, against a Kafka round trip.
- Three deploys: this change, then the drop of `Position` at T3, with the parcel conversion between them.
- `Be.Vlaanderen.Basisregisters.GrAr.CrsTransform` is added to `Consumer.Address`. It is already pinned at
  26.0.0 for `Api.Oslo`, so no package moves.
- Tests must assert SRIDs and coordinates explicitly. `FakeConsumerAddressContext` runs on the EF in-memory
  provider and NTS ignores SRID entirely, so an in-memory `Contains` will never reproduce SQL Server's
  `NULL`-on-mismatch. A test that relies on a mismatch failing will pass in memory and fail in production.
- `WithExtendedWkbGeometryPoint` now generates coordinates inside Flanders rather than arbitrary ones,
  because the guard above refuses a position it cannot transform. `WithExtendedWkbGeometryPointLambert2008`
  is its Lambert 2008 counterpart, mirroring the polygon fixtures ADR 0003 added.

### Resolved: the GRB importer's write side

This section recorded three things still to do. One of them is done, and the other two were answered
differently by [ADR 0005](0005-lambert2008-event-store-transformation.md). All three are recorded here
rather than deleted, because the reasoning is what the next reader needs.

#### Fix before transforming, and guard the result — done

`LambertTransformation.EnsureCoordinatesAreInCoordinateSystem` returns any geometry that is not `IsValid`
**untouched**, so an invalid parcel would have SRID 3812 stamped onto Lambert 72 coordinates and written
into the parcel event store, breaking the premise ADR 0003 rests on: that the bytes carry the truth.
Invalid parcel polygons do occur — `FindAddressesWithinGeometry` runs `GeometryFixer.Fix` for exactly that
reason, and `GeometryHelpers.InValidNTSButValidSqlPolygon` exists as a fixture.

`GeometryReferenceSystem.ToReferenceSystem` now runs `GeometryFixer.Fix` before it transforms, and throws
if the result's coordinates did not land in the target system's envelope.

**In `ToReferenceSystem`, not in the handlers.** This is the part worth recording. ADR 0005 makes that
function the single place that decides how a geometry moves between the two systems, precisely so that the
migrator and the GRB importer cannot disagree — and `Parcel.TransformToLambert2008()` goes through it too,
so it had the same hole. Fixing only in `ImportParcelHandler` and `ChangeParcelGeometryHandler` would have
left the migrator transforming an invalid geometry unfixed while the importer fixed it first, and the two
would then produce different bytes for the same parcel. That is exactly the divergence ADR 0005 warns
about: the first GRB import after the conversion would emit a `ParcelGeometryWasChanged` for every parcel
whose polygon is invalid in NTS but valid in SQL Server.

**Only on the transform branch.** When the coordinates are already in the target system, `ToReferenceSystem`
relabels or returns the geometry unchanged, and fixes nothing. That is deliberate. Today the event store is
Lambert 72 and GRB delivers Lambert 72, so nothing is transformed and nothing is fixed — an import that
changed nothing still produces byte-identical geometry, and no invalid parcel is quietly rewritten on the
first run after this change. Once the toggle flips every import transforms anyway, so the fix rides along
with a rewrite that was already happening and costs no additional events.

**The guard is separate from the fix**, because they catch different failures. Fixing removes the reason a
transform would decline; the guard catches it declining anyway. A transform that did not happen is
indistinguishable downstream from one that did — the geometry carries the target SRID either way — so
immediately after is the only place it can be caught. A geometry whose coordinates fall outside Flanders in
both systems now stops the importer instead of being persisted ~500 km from where the parcel is, which is
the same rule `ParcelConsumerItem.SetGeometry` already follows in building-registry.

**Neither half reads the SRID.** Worth stating, because the event store still holds geometries written
before it wrote EWKB — plain WKB from NTS's `AsBinary()`, carrying no SRID at all — and those are Lambert 72
by definition. Both the branch decision and the guard go through `ReferenceSystemOfCoordinates`, which reads
where the coordinates fall, so an unlabelled geometry transforms like any other and comes out labelled;
the transformation fixes the missing label as a side effect. Nothing reaches this method unlabelled today,
because every caller goes through `WKBReaderFactory.CreateForEwkb`, which falls back to the Lambert 72
reader and stamps 31370 — but the method does not depend on that, and must not start to. `IsSupported`
stays strict by contrast: it is a predicate on a label, and at the write boundary an unlabelled geometry
should be rejected rather than assumed, which is why `GuardPolygon` keeps refusing one.

`GeometryReferenceSystemTests` covers both halves, including the two easiest to lose in a later refactor:
an invalid geometry already in the target system comes back untouched, asserted by reference rather than by
value so that a fix creeping onto that branch fails the test; and an unlabelled geometry — SRID `-1`, which
is what a plain `WKBReader` reports — transforms correctly rather than being rejected or mislabelled.

#### The GML reader pinned to Lambert 72 — answered differently

This section named `GmlHelpers.CreateGmlReader()` and `GmlHelpers.GmlToExtendedWkbGeometry` as "the other
half of that change". ADR 0005 solved it another way: nothing on this path trusts the SRID label at all.
`GrbXmlReader` still reads every GRB polygon through a GMLReader built on the Lambert 72 geometry factory,
so a Lambert 2008 delivery would arrive carrying SRID 31370 — and both
`GeometryReferenceSystem.ReferenceSystemOfCoordinates` and `ConsumerAddressContext.FindAddressesWithinGeometry`
decide from the coordinates instead, relabelling rather than transforming when the label is the only thing
wrong. The reader needs no change, and pinning it to something else would not help.

#### The polygon handed to the address lookup — superseded

This section required the address lookup to receive the post-transform polygon, so that the addresses found
match the geometry stored against them. ADR 0005 reversed it: `FindAddressesWithinGeometry` is given the
geometry **as GRB delivered it**, so which addresses a parcel gets does not change with the parcel event
store's reference system. The lookup dispatches on coordinates and is correct either way, and the property
worth having is the one ADR 0005 chose — that converting the event store does not silently change which
addresses attach to a parcel.

#### What this means for the conversion sequence

Reversing the address lookup's input is what moved the sequence table at the top of this ADR, so the
consequence is recorded here as well as there.

The original table had T2 — the importer querying the Lambert 2008 column — strictly following the parcel
conversion, because the importer was to query with the converted parcel polygon. It no longer does, so the
Lambert 2008 branch of `FindAddressesWithinGeometry` is reached only when the coordinates handed to it are
Lambert 2008, and that happens when the GRB reader is switched.

**The two conversions are therefore independent, and the address and parcel migrators can run
concurrently** — which is what the revised table records. What still has to be true before the GRB reader is
switched is that `PositionLambert2008` is complete, and `GuardLambert2008PositionsAreComplete` enforces that
rather than leaving it to the ordering, which is why it is a guard rather than a comment.
