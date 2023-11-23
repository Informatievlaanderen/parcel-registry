namespace ParcelRegistry.Tests.AggregateTests.WhenAttachingParcelAddress
{
    using Api.BackOffice.Abstractions.Extensions;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Fixtures;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressAlreadyAttached : ParcelRegistryTest
    {
        public GivenAddressAlreadyAttached(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenNothing()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var command = new AttachAddressBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Realized)
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
