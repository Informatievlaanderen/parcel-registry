namespace ParcelRegistry.Tests.AggregateTests.WhenDetachingAddressBecauseAddressWasRetired
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
            var command = new DetachAddressBecauseAddressWasRetiredBuilder(Fixture)
                .WithAddress(1)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
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
