namespace ParcelRegistry.Tests.AggregateTests.WhenAttachingParcelAddress
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressNotAttached : ParcelRegistryTest
    {
        public GivenAddressNotAttached(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenParcelAddressWasAttachedV2()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(111);

            var command = new AttachAddress(
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
                    new AddressPersistentLocalId(123),
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
                    new ParcelAddressWasAttachedV2(command.ParcelId, new VbrCaPaKey(parcelWasMigrated.CaPaKey), command.AddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                Fixture.Create<ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    new AddressPersistentLocalId(123),
                    new AddressPersistentLocalId(456),
                    new AddressPersistentLocalId(789),
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var addressPersistentLocalId = new AddressPersistentLocalId(111);
            var parcelAddressWasAttachedV2 = new ParcelAddressWasAttachedV2(Fixture.Create<ParcelId>(), new VbrCaPaKey(parcelWasMigrated.CaPaKey), addressPersistentLocalId);
            ((ISetProvenance)parcelAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, parcelAddressWasAttachedV2 });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(4);
            sut.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId);
            sut.LastEventHash.Should().Be(parcelAddressWasAttachedV2.GetHash());
        }
    }
}
