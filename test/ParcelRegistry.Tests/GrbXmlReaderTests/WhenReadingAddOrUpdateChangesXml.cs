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

    public sealed class WhenReadingAddOrUpdateChangesXml
    {
        private readonly List<GrbParcel> _parcels;
        private readonly GMLReader _gmlReader;

        public WhenReadingAddOrUpdateChangesXml()
        {
            var grbUpdateXmlReader = new GrbXmlReader();
            _parcels = grbUpdateXmlReader.Read($"{AppContext.BaseDirectory}/GrbXmlReaderTests/AdpAdd.gml").ToList();

            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        [Fact]
        public void ThenUpdatedParcelsAreMapped()
        {
            _parcels.Should().HaveCount(11);
        }

        [Fact]
        public void ThenAddParcelIsMapped()
        {
            var firstParcel = _parcels.First();
            firstParcel.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("11004A0479/00N000");
            firstParcel.GrbCaPaKey.VbrCaPaKey.Should().Be("11004A0479-00N000");
            firstParcel.Geometry.Should().Be(_gmlReader.Read(
                "<gml:Polygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>160149.518034272,206784.003217537 160156.640274271,206787.933329538 160156.363026276,206789.099153541 160155.198994271,206794.390225545 160146.276882268,206790.78644954 160132.646930255,206785.393297538 160133.848082259,206783.230225537 160137.195026264,206777.203217532 160149.518034272,206784.003217537</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon>"));
            firstParcel.Geometry.Should().BeOfType<Polygon>();
            firstParcel.Version.Should().Be(1);
        }

        [Fact]
        public void ThenUpdatedParcelIsMapped()
        {
            var changeRequest = _parcels.Skip(9).First();
            changeRequest.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("44362B0107/00C010");
            changeRequest.GrbCaPaKey.VbrCaPaKey.Should().Be("44362B0107-00C010");
            changeRequest.Geometry.Should().Be(_gmlReader.Read(
                "<gml:Polygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>107285.567917727,192186.659015447 107289.996077731,192188.869319446 107291.525165729,192189.632583447 107292.592557736,192190.165383447 107294.694445737,192191.214535449 107295.241645738,192191.48768745 107289.05463773,192207.973063461 107285.303725727,192217.966791466 107284.302253731,192217.608391467 107281.197741725,192216.497351468 107275.890093721,192214.597831465 107275.882925719,192214.594759464 107274.906029724,192214.132423464 107278.111149721,192204.08288746 107278.789549723,192201.956039455 107281.915821724,192192.150727451 107283.830061726,192186.147335447 107283.863981724,192186.041031446 107285.567917727,192186.659015447</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon>"));
            changeRequest.Geometry.Should().BeOfType<Polygon>();
            changeRequest.Version.Should().Be(3);
        }
    }
}
