namespace ParcelRegistry.Tests.AggregateTests.WhenDetachingAddressBecauseAddressWasRejected
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Consumer.Address;
    using Fixtures;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Parcel;
    using Parcel.Events;
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

            var command = new DetachAddressBecauseAddressWasRejectedBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(command.AddressPersistentLocalId)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Current,
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

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

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(addressPersistentLocalId)
                .WithAddress(addressPersistentLocalId)
                .WithAddress(456)
                .Build();

            parcelWasMigrated.AddressPersistentLocalIds.Count(x => x == addressPersistentLocalId).Should().Be(2);

            var parcelAddressWasDetachedV2 = new ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, parcelAddressWasDetachedV2 });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(1);
            sut.AddressPersistentLocalIds.Should().NotContain(addressPersistentLocalId);
            sut.LastEventHash.Should().Be(parcelAddressWasDetachedV2.GetHash());
        }
    }
}
