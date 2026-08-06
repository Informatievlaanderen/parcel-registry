namespace ParcelRegistry.Projections.Integration.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using Parcel;

    public static class ParcelMapper
    {
        /// <summary>
        /// Reads a persisted geometry in the reference system its EWKB carries, rather than assuming one.
        /// The SRID travels into the PostGIS <c>geometry</c> column, so a row says which Lambert system it
        /// is in and <c>ST_SRID</c> can be branched on. See ADR 0003.
        /// </summary>
        public static Geometry MapExtendedWkbGeometryToGeometry(string extendedWkbGeometry)
        {
            var extendedWkb = extendedWkbGeometry.ToByteArray();

            return WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);
        }

        public static string ConvertFromParcelStatus(this ParcelStatus status)
        {
            if (status == ParcelStatus.Retired)
                return PerceelStatus.Gehistoreerd.ToString();

            return PerceelStatus.Gerealiseerd.ToString();
        }
    }
}
