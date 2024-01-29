namespace ParcelRegistry.Projections.Integration.Converters
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using NetTopologySuite.Geometries;
    using Parcel;

    public static class ParcelMapper
    {
        public static Geometry MapExtendedWkbGeometryToGeometry(string extendedWkbGeometry)
        {
            var geometry = WKBReaderFactory.Create().Read(new ExtendedWkbGeometry(extendedWkbGeometry));
            return geometry;
        }

        public static string ConvertFromParcelStatus(this ParcelStatus status)
        {
            if (status == ParcelStatus.Retired)
                return PerceelStatus.Gehistoreerd.ToString();

            return PerceelStatus.Gerealiseerd.ToString();
        }
    }
}
