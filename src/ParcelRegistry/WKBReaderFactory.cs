namespace ParcelRegistry
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.IO;
    using GrArWKBReaderFactory = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory;

    // ReSharper disable once InconsistentNaming
    public static class WKBReaderFactory
    {
        /// <summary>
        /// Creates a reader for a persisted geometry, in the reference system the bytes themselves carry,
        /// so callers do not have to assume which one the event store writes.
        /// Geometries persisted before the event store recorded an SRID are read as Lambert 72.
        /// See ADR 0003.
        /// </summary>
        public static WKBReader CreateForEwkb(byte[] ewkb) =>
            ewkb.TryReadSrid(out _)
                ? GrArWKBReaderFactory.CreateForEwkb(ewkb)
                : GrArWKBReaderFactory.CreateForLambert72();
    }
}
