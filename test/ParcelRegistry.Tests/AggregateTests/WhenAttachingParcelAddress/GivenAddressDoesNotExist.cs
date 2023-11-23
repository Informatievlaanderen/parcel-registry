namespace ParcelRegistry.Tests.AggregateTests.WhenAttachingParcelAddress
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Fixtures;
    using Parcel;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressDoesNotExist : ParcelRegistryTest
    {
        public GivenAddressDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenThrowAddressNotFoundException()
        {
            var command = new AttachAddressBuilder(Fixture)
                .WithAddress(11111111)
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
                .Throws(new AddressNotFoundException()));
        }
    }
}
