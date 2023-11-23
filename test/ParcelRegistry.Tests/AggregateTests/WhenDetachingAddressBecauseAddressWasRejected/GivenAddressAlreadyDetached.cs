namespace ParcelRegistry.Tests.AggregateTests.WhenDetachingAddressBecauseAddressWasRejected
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Fixtures;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressAlreadyDetached : ParcelRegistryTest
    {
        public GivenAddressAlreadyDetached(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenNothing()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var command = new DetachAddressBecauseAddressWasRejectedBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Realized)
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
