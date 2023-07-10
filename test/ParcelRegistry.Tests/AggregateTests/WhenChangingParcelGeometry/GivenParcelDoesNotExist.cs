namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using System;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelDoesNotExist : ParcelRegistryTest
    {
        public GivenParcelDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowParcelIsRemovedException()
        {
            var command = new ChangeParcelGeometry(
                Fixture.Create<VbrCaPaKey>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given()
                .When(command)
                .Throws(new AggregateNotFoundException(new ParcelStreamId(command.ParcelId), typeof(Parcel))));
        }
    }
}
