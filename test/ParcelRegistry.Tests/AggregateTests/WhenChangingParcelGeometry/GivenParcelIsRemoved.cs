namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using ParcelRegistry.Api.BackOffice.Abstractions.Extensions;
    using ParcelRegistry.Parcel;
    using ParcelRegistry.Parcel.Commands;
    using ParcelRegistry.Parcel.Events;
    using ParcelRegistry.Parcel.Exceptions;
    using ParcelRegistry.Tests.Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using ParcelId = ParcelRegistry.Legacy.ParcelId;
    using ParcelStatus = Parcel.ParcelStatus;

    public class GivenParcelIsRemoved : ParcelRegistryTest
    {
        public GivenParcelIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenThrowParcelIsRemovedException()
        {
            var command = new ChangeParcelGeometry(
                Fixture.Create<VbrCaPaKey>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Retired,
                isRemoved: true,
                new List<AddressPersistentLocalId>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new ParcelIsRemovedException(command.ParcelId)));
        }
    }
}
