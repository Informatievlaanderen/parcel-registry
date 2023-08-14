namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using System;
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelDoesNotExist : ParcelRegistryTest
    {
        public GivenParcelDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenParcelWasImported()
        {
            var command = new ChangeParcelGeometry(
                Fixture.Create<VbrCaPaKey>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                new List<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given()
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelWasImported(command.ParcelId, command.VbrCaPaKey,  GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry())));
        }
    }
}
