namespace ParcelRegistry.Tests.AggregateTests.WhenReaddressingAddresses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void WithSourceAddressAttachedAndDestinationAddressNotAttached_ThenAttachAndDetach()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(sourceAddressPersistentLocalId)
                .WithAddress(2)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(
                    new ParcelStreamId(command.ParcelId),
                    new ParcelAddressesWereReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        new[] { destinationAddressPersistentLocalId },
                        new[] { sourceAddressPersistentLocalId },
                        command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void WithSourceAddressAttachedAndDestinationAddressAlreadyAttached_ThenOnlyDetach()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(2)
                .WithAddress(sourceAddressPersistentLocalId)
                .WithAddress(destinationAddressPersistentLocalId)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(
                    new ParcelStreamId(command.ParcelId),
                    new ParcelAddressesWereReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        Array.Empty<AddressPersistentLocalId>(),
                        new[] { sourceAddressPersistentLocalId },
                        command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void WithSourceAddressNotAttachedAndDestinationNotAttached_ThenOnlyAttach()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(2)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(
                    new ParcelStreamId(command.ParcelId),
                    new ParcelAddressesWereReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        new[] { destinationAddressPersistentLocalId },
                        Array.Empty<AddressPersistentLocalId>(),
                        command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void WithSourceAddressNotAttachedAndDestinationAddressAlreadyAttached_ThenNothing()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(2)
                .WithAddress(destinationAddressPersistentLocalId)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithTwoReaddressesAndAddressIsBothSourceAndDestination_ThenOneAttachAndOneDetach()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var secondAddressPersistentLocalId = new AddressPersistentLocalId(5);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(sourceAddressPersistentLocalId, firstAddressPersistentLocalId)
                .WithReaddress(secondAddressPersistentLocalId, sourceAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(secondAddressPersistentLocalId)
                .WithAddress(sourceAddressPersistentLocalId)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(
                    new ParcelStreamId(command.ParcelId),
                    new ParcelAddressesWereReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        new[] { firstAddressPersistentLocalId },
                        new[] { secondAddressPersistentLocalId },
                        command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void StateCheck()
        {
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(sourceAddressPersistentLocalId)
                .WithAddress(otherAddressPersistentLocalId)
                .Build();

            var @event = new ParcelAddressesWereReaddressed(
                Fixture.Create<ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                new[] { destinationAddressPersistentLocalId },
                new[] { sourceAddressPersistentLocalId },
                new []{ new AddressRegistryReaddress(new ReaddressData(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)) }
            );
            @event.SetFixtureProvenance(Fixture);

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, @event });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().Contain(destinationAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().NotContain(sourceAddressPersistentLocalId);
            sut.LastEventHash.Should().Be(@event.GetHash());
        }
    }
}
