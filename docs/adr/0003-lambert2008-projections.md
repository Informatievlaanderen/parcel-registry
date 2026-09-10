# 3. Read parcel geometries in the reference system they were persisted in

Date: 2026-08-06

## Status

Accepted

## Context

The event store will be converted from Lambert 72 (EPSG 31370) to Lambert 2008 (EPSG 3812). When that
lands, `ExtendedWkbGeometry` on the events carries SRID 3812 instead of 31370, and everything reading
those events has to cope. The conversion emits a geometry-change event per parcel, so every parcel is
converted; a mix of the two reference systems is real but bounded, and consumers have to survive it.

Geometries are persisted as EWKB, which carries its own SRID. So a reader never has to *assume* a
reference system — it only has to stop hardcoding one. This mirrors the decision address-registry took in
its ADR 0004, in [address-registry#1375](https://github.com/Informatievlaanderen/address-registry/pull/1375)
(not yet merged at the time of writing).

This ADR covers the read side of `Projections.Legacy`, `Projections.Integration`, `Projections.Feed` and
the syndication feed in `Api.Oslo`. Handled separately: `Consumer.Address`, the producers, and the write
side (`Parcel.GuardPolygon`, `GmlHelpers.GmlToExtendedWkbGeometry`, `ExtendedWkbGeometry.SridLambert72`).

Parcel-registry has far less to change than address-registry because most projections never touch the
geometry (see "Nothing to do" below). There is no parcel counterpart to address-registry's new WFS V3 /
WMS V4 projections, tables, views or recomputed spatial-index bounding boxes.

Neither projection is pinned: both follow whatever the event store writes. The syndication feed is the one
consumer that has to choose a reference system, because its GML cannot express one — so it is also the only
reason this repository takes a dependency on `Be.Vlaanderen.Basisregisters.GrAr.CrsTransform`.

### What the current code already does

Measured against the versions this repo pins (NetTopologySuite 2.6.0, GrAr.Common 24.1.0):

| Input | `WKBReaderFactory.CreateForLambert72().Read(...)` |
|---|---|
| EWKB with SRID 31370 | SRID 31370, coordinates unchanged |
| EWKB with SRID 3812 | **SRID 3812**, coordinates unchanged |
| WKB with no SRID | SRID 31370 |

`WKBReader` takes the SRID from the bytes; the factory only supplies the default for SRID-less input. And
`GeometryExtensions.ConvertToGml` derives `srsName` from `geometry.SRID`, so it already emits
`.../EPSG/0/3812` for a Lambert 2008 geometry.

Both call sites in scope are therefore already correct for the conversion, today, unchanged. That is worth
recording plainly: this change alters no output while the event store still holds Lambert 72.

It is also not something to rely on. As address-registry's ADR 0004 puts it, reading Lambert 2008 correctly
through the Lambert 72 factory "is an accident of the current precision models, not a contract" — both GrAr
factories happen to use a floating precision model, and a fixed one would snap coordinates. The call sites
are moved off it so the code states its intent, and pinned by tests so the property cannot be lost
silently.

## Decision

### `ParcelRegistry.WKBReaderFactory.CreateForEwkb`

`GrAr.Common`'s `WKBReaderFactory.CreateForEwkb` throws `ArgumentException("No SrID found in EWKB")` when
the bytes carry no SRID. Everything written through `ExtendedWkbGeometry.CreateEWkb` does carry one — it
rejects `SRID <= 0` and writes with `HandleSRID = true` — but its `byte[]` and hex constructors do not
enforce that, and geometries predating the event store writing EWKB do not carry an SRID. The syndication
swagger examples in `ParcelSyndicationResponse` document both shapes side by side: one payload begins
`0103000000…` (no SRID flag), another `01030000208A7A0000…` (flag `0x20`, SRID `0x7A8A` = 31370).

`ParcelRegistry.WKBReaderFactory.CreateForEwkb` wraps it and falls back to the Lambert 72 reader in that
case, matching what `ExtendedWkbGeometry.CreateEWkb` already assumes for SRID-less input. **This is the
single place where "no SRID means Lambert 72" is decided**, and both consumers below read through it.

It shares its name with `Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory`, which
matches address-registry but needs care. C# resolves a simple name by walking outward from the innermost
namespace, and **at each level it consults that declaration's `using` directives before moving out**. So in
a file that imports the GrAr namespace, `WKBReaderFactory` binds to *GrAr's* — not to
`ParcelRegistry.WKBReaderFactory` from the enclosing namespace — with no ambiguity error and no warning.

That is a silent trap, because the two differ in exactly the case that matters: GrAr's throws on SRID-less
EWKB where ours falls back to Lambert 72.

- `ParcelMapper` imports nothing that declares the name, so the enclosing-namespace type is reached and
  the plain `using` was dropped.
- `ParcelDetailProjections` must import `GrAr.Common.NetTopology` for `ConvertToGml`, so it carries an
  explicit `using WKBReaderFactory = ParcelRegistry.WKBReaderFactory;` alias. A using-alias is consulted
  before imported namespaces, so this pins the binding in the file rather than leaving it to be inferred.

`GivenGeometryInEitherReferenceSystem.WhenPersistedWithoutSrid_ThenGmlIsLambert72` is what catches this:
removing the alias makes it fail with `ArgumentException: No SrID found in EWKB` rather than compiling to
something subtly different.

### `Projections.Integration`

`ParcelMapper.MapExtendedWkbGeometryToGeometry` reads through `CreateForEwkb`. It is the single call site
shared by `ParcelLatestItemV2Projections` and `ParcelVersionProjections`, so it is the parcel counterpart
of address-registry's `PositionReader`.

Npgsql's NetTopologySuite plugin writes the geometry's SRID into the PostGIS `geometry` column, so a row
carries the reference system the event store wrote and `ST_SRID` can be branched on. **The column is
deliberately allowed to hold both.** This database is consumed entirely outside this repository; the
consequences for those consumers are set out below.

### `Projections.Legacy`

`ParcelDetailProjections` reads through `CreateForEwkb` — per geometry, rather than from a reader cached in
the constructor — and lets `ConvertToGml` emit the matching `srsName`. The `Gml` column is self-describing
throughout, so a mix is legible rather than ambiguous. `GmlType` is unaffected by the reference system.

The column is not pinned to Lambert 72 because it has no reader to protect: nothing in `src/` or `test/`
reads `Gml` or `GmlType` outside the projection that writes them, and the Oslo detail and list responses
carry no geometry at all. That is also why no `ParcelDetailV3` is needed.

### `Api.Oslo` syndication: the caller picks, through `objectCrs`

The `/percelen/sync` feed is the one place that cannot simply follow the event store. Its GML comes from
`GrAr.Legacy.SpatialTools.GmlMultiSurfaceBuilder` / `PolygonBuilder`, whose `GmlPolygon` and
`GmlMultiSurface` types have **no `srsName` member at all** — the object's geometry is a bare `posList`. So
the feed cannot say which reference system it is in, and letting the object silently follow the event store
would move every consumer's coordinates ~500 km with nothing in the payload to signal it. This one cannot
be solved downstream.

A new filter, `objectCrs`, therefore makes the choice the caller's:

- `3812` → the object's geometry is emitted in Lambert 2008: transformed if the store still holds
  Lambert 72, passed through once it holds Lambert 2008.
- **anything else — an unrecognised value, an empty one, or no filter at all → Lambert 72**: passed through
  while the store holds Lambert 72, transformed back once it holds Lambert 2008.

The default is what makes the conversion invisible: every existing consumer keeps receiving Lambert 72
before and after, without changing a thing. Only a caller that opts in sees Lambert 2008.

An unrecognised value falls back rather than returning 400, so the feed never breaks on a typo. The cost is
that `objectCrs=EPSG:3812` silently yields Lambert 72; only the exact string `3812` (trimmed) selects
Lambert 2008. `ObjectCrs.ToSrid` is the single place that mapping lives, and
`GivenObjectCrsFilter.ThenOnlyTheExactValue3812SelectsLambert2008` pins the accepted spellings, so widening
them later is a one-line change with a test that documents it.

Two properties worth stating:

- **Only the object is reprojected.** The embedded `event` is the event store's own payload, emitted
  verbatim at every position, whatever `objectCrs` says. A feed replayed for auditing therefore still shows
  what was actually stored, including the conversion event itself.
- **Only a geometry that has to move is touched.** One already in the requested system is passed through,
  so no rounding is applied to it and today's output is byte-for-byte unchanged. A transformed geometry is
  rounded to 2 decimals, the centimetre precision coordinates are persisted at and the transform is
  accurate to, rather than carrying floating point noise into an 11-decimal `posList`.

A transformed geometry is handed back to the GML builders as EWKB because they take bytes and a reader
rather than a geometry, and their polygon mapping is `internal`. That round trip buys reuse of the single
existing GML serialisation instead of a second, divergent copy.

`ToRequestedCrs` therefore returns the bytes to hand the builder alongside the parsed geometry, and on the
untransformed path those are the persisted bytes themselves. Re-serializing there would have cost a write
and a second read per feed item — up to 100 per page, on the default path that every existing consumer is
on — and would have made "byte-for-byte unchanged" rest on the WKB round trip being lossless rather than on
nothing having been touched. The geometry is still parsed on that path, because which builder to call
depends on whether it is a `Polygon` or a `MultiPolygon`.

### `Projections.Feed`

Two different things in this repository are called a feed. The one above is the *syndication* feed
(`/percelen/sync`), which serves geometry. This is the *change* feed (`/percelen/wijzigingen`), which
serves CloudEvents — and it carries no geometry at all. `ParcelFeedProjections` stores the raw EWKB hex in
`ParcelDocument.GeometryAsExtendedWkb` and never parses it, `ParcelController` V3 hands out
`ParcelFeedItem.CloudEventAsString` verbatim, and nothing in between formats a coordinate.

The geometry is nevertheless read on the way in, once per cloud event, and it is the only reason this
projection is in scope.

#### The reference system reaches the change feed through the NIS codes

`AddCloudEvent` calls `GetNisCodes`, which asks `MunicipalityGeometryRepository.GetOverlappingNisCodes`
which municipalities the parcel's geometry intersects. Those NIS codes go into the CloudEvent and are
frozen into `CloudEventAsString` — the decision is made once, at projection time, and no consumer can
revisit it. That makes this the second place, after the syndication object, where getting the reference
system wrong could not be repaired downstream.

**The match is SRID-filtered, not transformed.** The repository caches every municipality boundary *twice*,
once per reference system, and selects with `m.Srid == srid && m.Geometry.Intersects(parcelGeometry)`. So
a parcel is compared against boundaries already expressed in its own system and nothing is reprojected at
read time. A `ParcelDocuments` table holding both systems during the conversion window therefore resolves
correctly row by row, with no window in which NIS codes are wrong.

SRID-less legacy geometries take the same route as everywhere else — `TryReadSrid` fails, the reader
becomes `CreateForLambert72()` and `srid` is set to Lambert 72 explicitly, so the filter matches the
Lambert 72 boundaries. Note that the `CreateForEwkb` on the other branch is *GrAr's*, not
`ParcelRegistry.WKBReaderFactory`: the file imports `GrAr.Common.NetTopology`, which outranks the
enclosing namespace for the simple name. That is correct here only because the SRID-less case never
reaches it — it is handled by the branch above, which is what `ParcelRegistry.WKBReaderFactory` exists to
do elsewhere.

#### The Lambert 2008 boundaries come from municipality-registry

`integration_municipality.municipality_geometries` and `municipality_geometries_2019` are not this
repository's tables. Their `geometry_lambert08` columns were added by municipality-registry's
`20260317055836_AddLambert08` (`ST_Transform(geometry, 3812)`, backfilled and then `SET NOT NULL`) and
`20260402123429_AddGeometries2019`. Both are `NOT NULL`, so there is no half-populated state to guard
against, and the repository's `SELECT` names the column unconditionally — this projection cannot start
without it. That makes it a hard cross-repository dependency, already satisfied, that nothing in this
repository would otherwise reveal. It is recorded here because it is invisible from the code and would
have to be re-derived by whoever next changes either side.

The cache is loaded once per process, on the first cloud event, and never refreshed.

#### Removed parcels close the loop

Per [ADR 0005](0005-lambert2008-event-store-transformation.md) a removed parcel is never transformed, so
its document keeps Lambert 72 permanently. That cannot produce a mismatched lookup: `ParcelWasMigrated`
adds the document and then returns *before* `AddCloudEvent` when `IsRemoved`, and every mutating method on
the aggregate calls `GuardParcelNotRemoved`, so no further event ever reaches that document. Its geometry
is stored and never read again — which is the same reason the transformation skips it.

#### Not covered by tests

`ParcelFeedProjectionsTests` mocks `IMunicipalityGeometryRepository`, and nothing else exercises
`MunicipalityGeometryRepository`. The SRID branch, the Lambert 72 fallback and the `m.Srid == srid` filter
therefore have no test at all — including no equivalent of the `GivenGeometryInEitherReferenceSystem` /
`GivenEventStoreInEitherReferenceSystem` fixtures the rest of this work is pinned by. Testing it needs a
PostGIS database rather than a fixture, which is why it was not done here; it is the one gap this section
leaves open.

### Nothing to do

Recorded so it does not have to be re-derived. All of the following store no geometry and handle
`ParcelGeometryWasChanged` as a version-timestamp bump only:

- `Projections.Wfs`, `Projections.Extract` (the parcel shapefile extract has no shape content at all;
  `ParcelLinkExtractProjections` maps the event to `DoNothing`), `Projections.LastChangedList`,
  `Projections.BackOffice`.

`Projections.Feed` was already reference-system agnostic too, but it does read geometries and has a
dependency worth recording, so it has a section of its own above rather than a line here.

### End state per table, and no rebuild anywhere

Both projections touch `Geometry` only on geometry-bearing events (`ParcelWasMigrated`,
`ParcelWasImported`, `ParcelGeometryWasChanged`, `ParcelWasCorrectedFromRetiredToRealized`). Because the
conversion emits such an event for every parcel, no projection rebuild is required:

- **`ParcelLatestItemV2`** — one row per parcel, every row rewritten by its conversion event. Converges to
  uniformly Lambert 2008; mixed only for the duration of the conversion run.
- **`ParcelDetail.Gml`** — same shape, same outcome: uniformly `srsName` 3812 afterwards.
- **`ParcelVersion`** — stays mixed **permanently, by design.** It appends a history row per event, and
  `CreateNewParcelVersion` copies the previous row's geometry forward, so version rows written before the
  conversion keep SRID 31370 for good.

`ParcelVersion` is deliberately left alone. It is a history of what the register actually held, and it
genuinely held Lambert 72 up to the conversion event and Lambert 2008 after it. Rebuilding it would rewrite
the reference system of rows describing past states — that is, falsify the history — to buy a uniformity
nothing needs, since the version history is not queried geographically. Address-registry took the same
position for its Legacy `Position` column: "Every reader must handle both regardless, so no rebuild is
required for correctness."

**This is a durable property of the table, not a transient window.** It is recorded here explicitly because
a future reader will otherwise file it as a bug and "fix" it with a rebuild.

## Consequences

- While the event store holds Lambert 72, output is byte-for-byte what it was. All new behaviour is on the
  3812 path, which no production data reaches yet. The change is therefore independent of conversion
  timing.
- SRID-less legacy geometries continue to read as Lambert 72, in one place, deliberately, instead of by
  accident of which factory a call site happened to pick.
- Consumers of the Integration PostGIS database must branch on `ST_SRID`, with obligations that differ per
  table:

  | Table | Mixed for | Consumer obligation |
  |---|---|---|
  | `ParcelLatestItemV2` | the conversion window | branch transiently; uniformly 3812 afterwards |
  | `ParcelVersion` | permanently, by design | branch forever, or never query it geographically |

- While a table is mixed, three PostGIS specifics apply:
  - **The GIST index stays valid.** `gist_geometry_ops_2d` indexes each row's 2D bounding box in raw
    coordinate space and ignores SRID, so both systems coexist without corrupting it. No reindex is needed
    and inserts do not fail.
  - **The predicate functions are what break.** `ST_Within`, `ST_Intersects`, `ST_DWithin` and friends
    raise `ERROR: Operation on mixed SRID geometries` when their operands disagree. The column is plain
    `geometry` with no SRID constraint, so Postgres accepts the mix silently and the breakage surfaces
    later, in the consumer's queries.
  - **Branching costs the index.** PostgreSQL does not guarantee left-to-right evaluation of `AND`, so a
    guard like `ST_SRID(g) = 31370 AND ST_Within(g, ref)` can still hit the error. `CASE` is the documented
    way to force evaluation order, but it hides the `&&` that `ST_Within` expands to, which is what the
    GIST index accelerates. Expect sequential scans, and time any view refresh against a mixed table
    *before* the freeze rather than during it.
- `ParcelVersion` carries a GIST index on `Geometry`. If the version history is genuinely never queried
  geographically, that index is paying for writes nobody reads, and under a permanent mix it can no longer
  serve a plain `ST_Within` anyway. Worth revisiting separately.
- No EF migrations and no schema changes: no table, column or index is added or altered.
- The change feed's CloudEvents are unaffected in shape — they carry no geometry — but their NIS codes
  depend on municipality-registry keeping `geometry_lambert08` populated in
  `integration_municipality.municipality_geometries` and `municipality_geometries_2019`. Anything that
  drops or stops maintaining that column breaks this repository's change feed, silently as far as this
  repository is concerned.
- The syndication feed gains an `objectCrs` filter. Callers that do not use it are unaffected in either
  direction. `ParcelSyndicationFilter` is populated from the `X-Filtering` header, so exposing `objectCrs`
  as a query parameter needs the same gateway mapping that `embed` and `from` already rely on — that part
  lives outside this repository.
- `Be.Vlaanderen.Basisregisters.GrAr.CrsTransform` 24.1.0 is added, referenced only by `Api.Oslo`. It
  depends on exactly the `GrAr.Common` 24.1.0 already pinned, so no other package moves. Note that the
  25.x line of CrsTransform requires `GrAr.Common` >= 25.7.0 and would drag the whole GrAr family with it.
- **A geometry that is not `IsValid` is not transformed.** `LambertTransformation.EnsureCoordinatesAreInCoordinateSystem`
  returns such a geometry untouched, so an invalid Lambert 2008 parcel would be emitted with Lambert 2008
  coordinates to a caller that asked for Lambert 72 — and, there being no `srsName`, silently. Invalid
  parcel polygons do occur: `ConsumerAddressContext.FindAddressesWithinGeometry` runs `GeometryFixer.Fix`
  for exactly that reason, and `GeometryHelpers.InValidNTSButValidSqlPolygon` exists as a fixture. This is
  left as-is rather than papered over with a fixer, which would change the emitted shape; it needs a
  decision of its own before the store is converted. It does not affect anything today, while the store
  still holds Lambert 72 and no transform runs on the default path.
- Still to do for the conversion, each in its own change: `Consumer.Address`, which reads *address*
  positions off Kafka with a reader pinned to Lambert 72 and is therefore driven by address-registry's
  conversion, not this one; the producers; and the write side.
