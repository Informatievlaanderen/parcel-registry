namespace ParcelRegistry.Tests.AggregateTests.WhenRetiringParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Parcel.Exceptions;
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
            var command = new RetireParcelV2(
                Fixture.Create<VbrCaPaKey>(),
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
