namespace ParcelRegistry.Tests
{
    using Consumer.Address;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using NetTopologySuite.IO.GML2;
    using Parcel;

    public static class GeometryHelpers
    {
        public const string ValidGmlPolygon =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:exterior>" +
            "<gml:LinearRing>" +
            "<gml:posList>" +
            "140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031" +
            "</gml:posList>" +
            "</gml:LinearRing>" +
            "</gml:exterior>" +
            "</gml:Polygon>";

        public const string ValidGmlPolygon2 =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:exterior>" +
            "<gml:LinearRing>" +
            "<gml:posList>" +
            "100 100 200 100 200 200 100 200 100 100" +
            "</gml:posList>" +
            "</gml:LinearRing>" +
            "</gml:exterior>" +
            "</gml:Polygon>";
        public static Point ValidPoint1InPolgyon2 = new Point(150, 150);
        public static Point ValidPointOnEdgeOfPolygon2 = new Point(100, 100);
        public static Point PointOutsideOfValidPolygon2 = new Point(1, 1);

        public const string ValidGmlPolygon3 =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:exterior>" +
            "<gml:LinearRing>" +
            "<gml:posList>" +
            "300 300 400 300 400 400 300 400 300 300" +
            "</gml:posList>" +
            "</gml:LinearRing>" +
            "</gml:exterior>" +
            "</gml:Polygon>";
        public static Point ValidPoint1InPolgyon3 = new Point(250, 350);

        // Polygon is invalid because interior and exterior rings intersect
        public const string InValidGmlPolygon =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:exterior>" +
            "<gml:LinearRing>" +
            "<gml:posList>" +
            "140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031" +
            "</gml:posList>" +
            "</gml:LinearRing>" +
            "</gml:exterior>" +
            "<gml:interior>" +
            "<gml:LinearRing>" +
            "<gml:posList>" +
            "140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031" +
            "</gml:posList>" +
            "</gml:LinearRing>" +
            "</gml:interior>" +
            "</gml:Polygon>";

        public const string GmlPointGeometry =
            "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>103671.37 192046.71</gml:pos></gml:Point>";

        public const string SecondGmlPointGeometry =
            "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>103672.37 192046.71</gml:pos></gml:Point>";

        public static ExtendedWkbGeometry ToExtendedWkbGeometry(MultiPolygon multiPolygon)
        {
            return ExtendedWkbGeometry.CreateEWkb(new WKBWriter().Write(multiPolygon));
        }

        private static readonly WKBWriter WkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
        public static byte[] ExampleWkb { get; }
        public static byte[] ExampleExtendedWkb { get; }

        static GeometryHelpers()
        {
            var point = "POINT (141299 185188)";
            var geometry = new WKTReader { DefaultSRID = SpatialReferenceSystemId.Lambert72 }.Read(point);
            ExampleWkb = geometry.AsBinary();
            geometry.SRID = SpatialReferenceSystemId.Lambert72;
            ExampleExtendedWkb = WkbWriter.Write(geometry);
        }

        public static ExtendedWkbGeometry CreateFromWkt(string wkt)
        {
            var geometry = new WKTReader { DefaultSRID = SpatialReferenceSystemId.Lambert72 }.Read(wkt);
            return ExtendedWkbGeometry.CreateEWkb(WkbWriter.Write(geometry));
        }

        public static GMLReader CreateGmlReader() =>
            new GMLReader(
                new GeometryFactory(
                    new PrecisionModel(PrecisionModels.Floating),
                    ExtendedWkbGeometry.SridLambert72,
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY)));

        public static ExtendedWkbGeometry ToAddressExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = SpatialReferenceSystemId.Lambert72;

            return new ExtendedWkbGeometry(geometry.AsBinary());
        }

        public static Geometry ToGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = SpatialReferenceSystemId.Lambert72;

            return geometry;
        }

        public static Polygon ValidPolygon => (Polygon)ValidGmlPolygon.ToGeometry();
        public static Polygon ValidPolygon2 => (Polygon)ValidGmlPolygon2.ToGeometry();
        public static Polygon InValidPolygon => (Polygon)InValidGmlPolygon.ToGeometry();
    }
}
