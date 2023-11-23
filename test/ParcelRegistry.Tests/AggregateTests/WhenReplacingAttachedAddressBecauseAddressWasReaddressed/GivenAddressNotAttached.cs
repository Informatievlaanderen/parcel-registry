namespace ParcelRegistry.Tests.AggregateTests.WhenReplacingAttachedAddressBecauseAddressWasReaddressed
{
    using System.Collections.Generic;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using Parcel.Events;
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
        public void WithPreviousAddressAttached_ThenParcelAddressWasReplacedBecauseAddressWasReaddressed()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder(Fixture)
                .WithNewAddress(addressPersistentLocalId)
                .WithPreviousAddress(previousAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(previousAddressPersistentLocalId)
                .WithAddress(2)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        addressPersistentLocalId,
                        previousAddressPersistentLocalId))));
        }

        [Fact]
        public void WithPreviousAddressNotAttached_ThenParcelAddressWasReplacedBecauseAddressWasReaddressed()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder(Fixture)
                .WithNewAddress(addressPersistentLocalId)
                .WithPreviousAddress(previousAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(2)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        addressPersistentLocalId,
                        previousAddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(previousAddressPersistentLocalId)
                .WithAddress(otherAddressPersistentLocalId)
                .Build();

            var attachedParcelAddressWasReplacedBecauseAddressWasReaddressed =
                new ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder(Fixture)
                    .WithVbrCaPaKey(new VbrCaPaKey(parcelWasMigrated.CaPaKey))
                    .WithNewAddress(addressPersistentLocalId)
                    .WithPreviousAddress(previousAddressPersistentLocalId)
                    .Build();
            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, attachedParcelAddressWasReplacedBecauseAddressWasReaddressed });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().NotContain(previousAddressPersistentLocalId);
            sut.LastEventHash.Should().Be(attachedParcelAddressWasReplacedBecauseAddressWasReaddressed.GetHash());
        }
    }
}
