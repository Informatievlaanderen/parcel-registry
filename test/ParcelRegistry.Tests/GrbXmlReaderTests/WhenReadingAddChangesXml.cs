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

    public sealed class WhenReadingAddChangesXml
    {
        private readonly List<GrbParcel> _parcels;
        private readonly GMLReader _gmlReader;

        public WhenReadingAddChangesXml()
        {
            var grbAddXmlReader = new GrbAddXmlReader();
            _parcels = grbAddXmlReader.Read($"{AppContext.BaseDirectory}/GrbXmlReaderTests/AdpAdd.gml").ToList();

            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        [Fact]
        public void ThenAddedParcelsAreMapped()
        {
            _parcels.Should().HaveCount(9);
        }

        [Fact]
        public void ThenAddedParcelIsMapped()
        {
            var firstParcel = _parcels.First();
            firstParcel.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("11004A0479/00N000");
            firstParcel.GrbCaPaKey.VbrCaPaKey.Should().Be("11004A0479-00N000");
            firstParcel.Geometry.Should().Be(_gmlReader.Read(
                "<gml:Polygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>160149.518034272,206784.003217537 160156.640274271,206787.933329538 160156.363026276,206789.099153541 160155.198994271,206794.390225545 160146.276882268,206790.78644954 160132.646930255,206785.393297538 160133.848082259,206783.230225537 160137.195026264,206777.203217532 160149.518034272,206784.003217537</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon>"));
            firstParcel.Geometry.Should().BeOfType<Polygon>();
        }
    }
}
