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

    public sealed class WhenReadingDeleteChangesXml
    {
        private readonly List<GrbParcel> _parcels;
        private readonly GMLReader _gmlReader;

        public WhenReadingDeleteChangesXml()
        {
            var grbDeleteXmlReader = new GrbDeleteXmlReader();
            _parcels = grbDeleteXmlReader.Read($"{AppContext.BaseDirectory}/GrbXmlReaderTests/AdpDel.gml").ToList();

            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        [Fact]
        public void ThenUpdatedParcelsAreMapped()
        {
            _parcels.Should().HaveCount(6);
        }

        [Fact]
        public void ThenUpdatedParcelIsMapped()
        {
            var firstParcel = _parcels.First();
            firstParcel.GrbCaPaKey.CaPaKeyCrabNotation2.Should().Be("11922B0570/00H004");
            firstParcel.GrbCaPaKey.VbrCaPaKey.Should().Be("11922B0570-00H004");
            firstParcel.Geometry.Should().Be(_gmlReader.Read(
                "<gml:Polygon srsName=\"EPSG:31370\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:outerBoundaryIs><gml:LinearRing><gml:coordinates>165249.848085791,214284.005974721 165248.997013792,214287.019798722 165255.904597797,214289.062998723 165261.249365799,214290.643990725 165268.240917809,214292.712022725 165267.364949808,214295.620054729 165260.407381803,214293.686422728 165254.971477799,214292.175702725 165249.987413794,214290.790550724 165238.842005789,214287.693014722 165238.905237786,214287.469142724 165240.62101379,214281.397974718 165249.848085791,214284.005974721</gml:coordinates></gml:LinearRing></gml:outerBoundaryIs></gml:Polygon>"));
            firstParcel.Geometry.Should().BeOfType<Polygon>();
        }
    }
}
