namespace ParcelRegistry.Tests.AggregateTests.WhenDetachingParcelAddress
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Parcel.Exceptions;
    using BackOffice;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved : ParcelRegistryTest
    {
        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenThrowAddressIsRemovedException()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(11111111);

            var command = new DetachAddress(
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
            consumerAddress.AddAddress(addressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: true);

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new AddressIsRemovedException()));
        }
    }
}
