namespace ParcelRegistry.Tests.GrbXmlReaderTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions.Extensions;
    using FluentAssertions;
    using Importer.Grb.Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;
    using Xunit;

    public sealed class WhenReadingFullXml
    {
        private readonly List<GrbParcel> _parcels;
        private readonly GMLReader _gmlReader;

        public WhenReadingFullXml()
        {
            var grbXmlReader = new GrbXmlReader($"{AppContext.BaseDirectory}/GrbXmlReaderTests/Adp_Full.gml");
            //var grbXmlReader = new GrbXmlReader($"C:\\Users\\xxx\\Downloads\\Adp_20230703_GML\\GML\\Adp.gml"); // use to test full download
            _parcels = grbXmlReader.Read().ToList();
            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        [Fact]
        public void ThenAllParcelsAreMapped()
        {
            _parcels.Should().HaveCount(13);
        }

        [Fact]
        public void ThenPolygonParcelIsMapped()
        {
            var firstParcel = _parcels.First();
            firstParcel.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("72033A0610/00A000");
            firstParcel.GrbCaPaKey.VbrCaPaKey.Should().Be("72033A0610-00A000");
            firstParcel.Geometry.Should().Be(_gmlReader.Read(
                "<gml:Polygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>228737.513985679,212959.675989803 228738.381953679,212966.35202181 228731.793985672,212967.208021808 228730.926081672,212960.531989805 228737.513985679,212959.675989803</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon>"));
            firstParcel.Geometry.Should().BeOfType<Polygon>();
        }

        [Fact]
        public void ThenMultiPolygonParcelIsMapped()
        {
            var lastParcel = _parcels.Last();
            lastParcel.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("71483D0187/00P000");
            lastParcel.GrbCaPaKey.VbrCaPaKey.Should().Be("71483D0187-00P000");
            lastParcel.Geometry.Should().Be(_gmlReader.Read(
                "<gml:MultiPolygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:polygonMember><gml:Polygon><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>215125.24511227,181678.313472182 215128.569400273,181672.497536179 215130.246392272,181669.565120175 215132.903416276,181664.919040173 215133.853688277,181666.271040175 215134.504824273,181667.197440173 215135.797816277,181669.037056178 215125.24511227,181678.313472182</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon></gml:polygonMember><gml:polygonMember><gml:Polygon><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>215118.347640261,181684.206464186 215118.175544262,181683.006784186 215125.24511227,181678.313472182 215118.347640261,181684.206464186</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon></gml:polygonMember></gml:MultiPolygon>"));
            lastParcel.Geometry.Should().BeOfType<MultiPolygon>();
            lastParcel.Geometry.As<MultiPolygon>().NumGeometries.Should().Be(2);
        }
    }
}
