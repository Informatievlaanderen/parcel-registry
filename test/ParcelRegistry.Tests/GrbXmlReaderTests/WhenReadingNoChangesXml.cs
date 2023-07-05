namespace ParcelRegistry.Tests.GrbXmlReaderTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Importer.Grb.Infrastructure;
    using Xunit;

    public sealed class WhenReadingNoChangesXml
    {
        private readonly List<GrbParcel> _parcels;

        public WhenReadingNoChangesXml()
        {
            var grbXmlReader = new GrbXmlReader();
            _parcels = grbXmlReader.Read($"{AppContext.BaseDirectory}/GrbXmlReaderTests/AdpAdd_NoChanges.gml").ToList();
        }

        [Fact]
        public void ThenNoParcelsAreMapped()
        {
            _parcels.Should().BeEmpty();
        }
    }
}
