namespace ParcelRegistry.Tests.GrbXmlReaderTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions.Extensions;
    using FluentAssertions;
    using Migrator.Parcel.Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;
    using Xunit;

    public sealed class WhenReadingUpdateChangesXml
    {
        private readonly List<GrbParcel> _parcels;
        private readonly GMLReader _gmlReader;

        public WhenReadingUpdateChangesXml()
        {
            var grbUpdateXmlReader = new GrbUpdateXmlReader($"{AppContext.BaseDirectory}/GrbXmlReaderTests/AdpAdd.gml");
            _parcels = grbUpdateXmlReader.Read().ToList();

            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        [Fact]
        public void ThenUpdatedParcelsAreMapped()
        {
            _parcels.Should().HaveCount(2);
        }

        [Fact]
        public void ThenUpdatedParcelIsMapped()
        {
            var firstParcel = _parcels.First();
            firstParcel.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("44362B0107/00C010");
            firstParcel.GrbCaPaKey.VbrCaPaKey.Should().Be("44362B0107-00C010");
            firstParcel.Geometry.Should().Be(_gmlReader.Read(
                "<gml:Polygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>107285.567917727,192186.659015447 107289.996077731,192188.869319446 107291.525165729,192189.632583447 107292.592557736,192190.165383447 107294.694445737,192191.214535449 107295.241645738,192191.48768745 107289.05463773,192207.973063461 107285.303725727,192217.966791466 107284.302253731,192217.608391467 107281.197741725,192216.497351468 107275.890093721,192214.597831465 107275.882925719,192214.594759464 107274.906029724,192214.132423464 107278.111149721,192204.08288746 107278.789549723,192201.956039455 107281.915821724,192192.150727451 107283.830061726,192186.147335447 107283.863981724,192186.041031446 107285.567917727,192186.659015447</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon>"));
            firstParcel.Geometry.Should().BeOfType<Polygon>();
        }
    }
}
