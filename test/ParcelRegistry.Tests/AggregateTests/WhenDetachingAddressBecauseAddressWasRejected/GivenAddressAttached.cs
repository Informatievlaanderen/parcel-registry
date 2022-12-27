namespace ParcelRegistry.Tests.AggregateTests.WhenDetachingAddressBecauseAddressWasRejected
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using BackOffice;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressAttached : ParcelRegistryTest
    {
        public GivenAddressAttached(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenAddressWasDetachedBecauseAddressWasRemoved()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var command = new DetachAddressBecauseAddressWasRejected(
                Fixture.Create<ParcelId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    command.AddressPersistentLocalId,
                    new AddressPersistentLocalId(456),
                    new AddressPersistentLocalId(789),
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressPersistentLocalId, Consumer.Address.AddressStatus.Current);

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasDetachedBecauseAddressWasRejected(command.ParcelId, new VbrCaPaKey(parcelWasMigrated.CaPaKey), command.AddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                Fixture.Create<ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    addressPersistentLocalId,
                    new AddressPersistentLocalId(456),
                    new AddressPersistentLocalId(789),
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var parcelAddressWasDetachedV2 = new ParcelAddressWasDetachedBecauseAddressWasRejected(Fixture.Create<ParcelId>(), new VbrCaPaKey(parcelWasMigrated.CaPaKey), addressPersistentLocalId);
            ((ISetProvenance)parcelAddressWasDetachedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, parcelAddressWasDetachedV2 });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().NotContain(addressPersistentLocalId);
            sut.LastEventHash.Should().Be(parcelAddressWasDetachedV2.GetHash());
        }
    }
}
