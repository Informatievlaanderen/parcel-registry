namespace ParcelRegistry.Tests.GrbXmlReaderTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Migrator.Parcel.Infrastructure;
    using Xunit;

    public sealed class WhenReadingNoChangesXml
    {
        private readonly List<GrbParcel> _parcels;

        public WhenReadingNoChangesXml()
        {
            var grbXmlReader = new GrbXmlReader($"{AppContext.BaseDirectory}/GrbXmlReaderTests/AdpAdd_NoChanges.gml");
            _parcels = grbXmlReader.Read().ToList();
        }

        [Fact]
        public void ThenNoParcelsAreMapped()
        {
            _parcels.Should().BeEmpty();
        }
    }
}
