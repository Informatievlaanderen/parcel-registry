namespace ParcelRegistry.Tests.AggregateTests.WhenImportingParcelGeometry
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using Fixtures;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists (ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenParcelGeometryImported()
        {
            // TODO: legacy parcelId and command.parcelId should be the same fixed
            var legacyParcelId = Fixture.Create<ParcelRegistry.Legacy.ParcelId>();
            var newParcelId = Fixture.Create<ParcelId>();

            var command = new ImportParcelGeometry(
                Fixture.Create<VbrCaPaKey>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            var parcelWasMigrated = Fixture.Create<ParcelWasMigrated>().WithClearedAddresses();

            // Assert
            Assert(new Scenario()
                .Given(new ParcelStreamId(command.ParcelId), parcelWasMigrated)
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelGeometryWasImported(
                        command.ParcelId,
                        new VbrCaPaKey(command.ParcelId),
                        command.Geometry)));
        }
    }
}
