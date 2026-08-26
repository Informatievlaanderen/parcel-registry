# 4. Hold consumed address positions in both reference systems

Date: 2026-08-26

## Status

Proposed

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

| | Event | `Addresses` table | GRB importer |
|---|---|---|---|
| T0 | This change is deployed | 72 complete, 08 empty | running, parcels in 72 |
| T0→T1 | Address event store converted; each address produced to Kafka and consumed within seconds | both columns fill | running, parcels in 72 |
| T1 | All addresses converted | both complete | running, parcels in 72 |
| T1→T2 | Parcel event store converted, parcel by parcel | idle | **paused** |
| T2 | Importer re-enabled against Lambert 2008 geometries | both complete | running, parcels in 08 |
| T3 | Lambert 72 column dropped | 08 only | running, parcels in 08 |

Two properties of this sequence are load-bearing:

- **Every address is converted, and each conversion is an event.** The address conversion is a full
  convert including removed addresses, emitting `AddressPositionCrsWasChanged` per address. That is a
  complete rewrite of this table, delivered for free, in seconds of consumer lag. It happens once.
- **T2 strictly follows T1.** No Lambert 2008 parcel polygon is ever queried before the Lambert 2008
  column is complete.

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
  correctly indexed from T0 to T3, so the importer keeps running through T0→T1. The T1→T2 pause is
  required by the parcel conversion itself and is not a cost of this design.
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
  only because parcels are still Lambert 72 in that window, and the guard above is what makes "only
  because" enforced rather than assumed.
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

### Still to do, and now on the critical path

The GRB importer's write side is out of scope here but is no longer independent of it. At T2 the importer
either asks GRB for Lambert 2008 GML or transforms Lambert 72 GML into Lambert 2008 before building the
event.

If it transforms, it transforms a **polygon**, and
`LambertTransformation.EnsureCoordinatesAreInCoordinateSystem` returns any geometry that is not `IsValid`
**untouched** — so an invalid parcel would have SRID 3812 stamped onto Lambert 72 coordinates and written
into the parcel event store, which breaks the premise ADR 0003 rests on, that the bytes carry the truth.
Invalid parcel polygons do occur: `FindAddressesWithinGeometry` runs `GeometryFixer.Fix` for exactly that
reason and `GeometryHelpers.InValidNTSButValidSqlPolygon` exists as a fixture. Fix before transforming,
and guard the result.

`GmlHelpers.CreateGmlReader()` and `GmlHelpers.GmlToExtendedWkbGeometry` are both pinned to Lambert 72 and
are the other half of that change.

The polygon handed to `FindAddressesWithinGeometry` must be the post-transform one — the same geometry
that goes into the event — so that the addresses found match the geometry stored against them.

ADR 0003 left the invalid-geometry-is-not-transformed hole open as needing "a decision of its own before
the store is converted". It still does, and it is now blocking.
