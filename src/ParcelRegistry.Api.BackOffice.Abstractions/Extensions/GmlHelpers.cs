namespace ParcelRegistry.Api.BackOffice.Abstractions.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.IO.GML2;

    public static class GmlHelpers
    {
        public static GMLReader CreateGmlReader() =>
            new GMLReader(NtsGeometryFactory.CreateGeometryFactoryLambert72());
    }
}
