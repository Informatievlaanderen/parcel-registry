namespace ParcelRegistry.Api.BackOffice.Abstractions.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.IO.GML2;
    using Parcel;

    public static class GmlHelpers
    {
        public static ExtendedWkbGeometry GmlToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return ExtendedWkbGeometry.CreateEWkb(geometry)!;
        }

        public static GMLReader CreateGmlReader() =>
            new GMLReader(NtsGeometryFactory.CreateGeometryFactoryLambert72());
    }
}
