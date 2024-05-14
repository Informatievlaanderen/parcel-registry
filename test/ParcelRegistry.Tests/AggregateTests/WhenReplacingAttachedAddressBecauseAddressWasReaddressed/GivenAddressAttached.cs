namespace ParcelRegistry.Tests.AggregateTests.WhenReplacingAttachedAddressBecauseAddressWasReaddressed
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Fixtures;
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
                .WithAddress(addressPersistentLocalId)
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
        public void WithPreviousAddressNotAttached_ThenNothing()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder(Fixture)
                .WithNewAddress(addressPersistentLocalId)
                .WithPreviousAddress(previousAddressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(2)
                .WithAddress(addressPersistentLocalId)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .ThenNone());
        }
    }
}
