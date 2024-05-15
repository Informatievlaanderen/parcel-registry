namespace ParcelRegistry.Tests.AggregateTests.WhenReaddressingAddresses
{
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
        public void WithSourceAddressAttached_ThenAttachAndDetach()
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
                        new ParcelAddressWasDetachedBecauseAddressWasReaddressed(
                            command.ParcelId,
                            new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                            sourceAddressPersistentLocalId),
                        new ParcelAddressWasAttachedBecauseAddressWasReaddressed(
                            command.ParcelId,
                            new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                            destinationAddressPersistentLocalId)
                ));
        }

        [Fact]
        public void WithPreviousAddressNotAttached_ThenNothing()
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
    }
}
