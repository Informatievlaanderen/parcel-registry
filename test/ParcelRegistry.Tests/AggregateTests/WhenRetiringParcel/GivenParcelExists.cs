namespace ParcelRegistry.Tests.AggregateTests.WhenRetiringParcel
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
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
        public void ThenParcelIsRetired()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var firstAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstParcelAddressWasAttached = new ParcelAddressWasAttachedV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithAddress(firstAddressPersistentLocalId)
                .Build();

            var secondAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondParcelAddressWasAttached = new ParcelAddressWasAttachedV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithAddress(secondAddressPersistentLocalId)
                .Build();

            var command = new RetireParcelV2(caPaKey, Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId),
                    parcelWasImported,
                    firstParcelAddressWasAttached,
                    secondParcelAddressWasAttached)
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasDetachedV2(parcelId, caPaKey, firstAddressPersistentLocalId),
                    new ParcelAddressWasDetachedV2(parcelId, caPaKey, secondAddressPersistentLocalId),
                    new ParcelWasRetiredV2(parcelId, caPaKey)));
        }

        [Fact]
        public void WithRetiredParcel_ThenNone()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var command = new RetireParcelV2(caPaKey, Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Retired)
                .Build();

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

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var parcelWasRetiredV2 = new ParcelWasRetiredV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithVbrCaPaKey(caPaKey)
                .Build();

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
