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

This ADR covers the read side of `Projections.Legacy` and `Projections.Integration` only. Handled
separately: `Api.Oslo/Parcel/Sync`, `Consumer.Address`, the producers, and the write side
(`Parcel.GuardPolygon`, `GmlHelpers.GmlToExtendedWkbGeometry`, `ExtendedWkbGeometry.SridLambert72`).

Parcel-registry has far less to change than address-registry because most projections never touch the
geometry (see "Nothing to do" below), and because **no parcel consumer in scope is pinned to Lambert 72** —
both projections follow whatever the event store writes. There is therefore no parcel counterpart to
address-registry's new WFS V3 / WMS V4 projections, tables, views or recomputed spatial-index bounding
boxes, and no dependency on `Be.Vlaanderen.Basisregisters.GrAr.CrsTransform`: nothing pins, so nothing
transforms.

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

### Nothing to do

Recorded so it does not have to be re-derived. All of the following store no geometry and handle
`ParcelGeometryWasChanged` as a version-timestamp bump only:

- `Projections.Wfs`, `Projections.Extract` (the parcel shapefile extract has no shape content at all;
  `ParcelLinkExtractProjections` maps the event to `DoNothing`), `Projections.LastChangedList`,
  `Projections.BackOffice`.

And already reference-system agnostic:

- `Projections.Feed` — `ParcelFeedProjections` stores the raw EWKB hex and never parses it;
  `MunicipalityGeometryRepository` already reads through `CreateForEwkb` with a Lambert 72 fallback and
  handles both systems.

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
- Still to do for the conversion, each in its own change: `Api.Oslo/Parcel/Sync` — its GML builders emit a
  bare `posList` with no `srsName`, so its implicit Lambert 72 contract cannot be satisfied downstream and
  must be handled at the source; `Consumer.Address`, which reads *address* positions off Kafka with a
  reader pinned to Lambert 72 and is therefore driven by address-registry's conversion, not this one; the
  producers; and the write side.
