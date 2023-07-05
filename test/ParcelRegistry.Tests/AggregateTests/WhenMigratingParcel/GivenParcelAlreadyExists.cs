namespace ParcelRegistry.Tests.AggregateTests.WhenMigratingParcel
{
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

    public class GivenParcelAlreadyExists : ParcelRegistryTest
    {
        public GivenParcelAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowInvalidOperationException()
        {
            var parcelWasMigrated = Fixture.Create<ParcelWasMigrated>();

            var command = new MigrateParcel(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.NewParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new AggregateSourceException($"Parcel with id {command.NewParcelId} already exists")));
        }
    }
}
