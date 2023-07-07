namespace ParcelRegistry.Tests.AggregateTests.WhenRetiringParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public void ThenParcelRetired()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var command = new RetireParcelV2(caPaKey, Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported)
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelWasRetiredV2(parcelId, caPaKey)));
        }

        [Fact]
        public void WhenAlreadyRetired_ThenNone()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var command = new RetireParcelV2(caPaKey, Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Retired,
                isRemoved: false,
                new List<AddressPersistentLocalId>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var parcelId = Fixture.Create<ParcelId>();
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                Fixture.Create<ExtendedWkbGeometry>());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var parcelWasRetiredV2 = new ParcelWasRetiredV2(parcelId, caPaKey);
            parcelWasRetiredV2.SetFixtureProvenance(Fixture);

            var parcel = new ParcelFactory(NoSnapshotStrategy.Instance,  new Mock<IAddresses>().Object).Create();
            parcel.Initialize(new object[]
            {
                parcelWasImported,
                parcelWasRetiredV2
            });

            parcel.ParcelStatus.Should().Be(ParcelStatus.Retired);
        }
    }
}
