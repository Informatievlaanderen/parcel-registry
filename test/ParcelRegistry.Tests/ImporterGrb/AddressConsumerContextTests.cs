namespace ParcelRegistry.Tests.ImporterGrb
{
    using AutoFixture;
    using BackOffice;
    using Consumer.Address;
    using FluentAssertions;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressConsumerContextTests : ParcelRegistryTest
    {
        public AddressConsumerContextTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Theory]
        [InlineData("Retired")]
        [InlineData("Rejected")]
        public void WhenAddressHasInvalidStatus_ThenExclude(string invalidStatus)
        {
            var fakeAddressConsumerContext = new FakeConsumerAddressContextFactory().CreateDbContext();

            var addressPersistentLocalId1 = Fixture.Create<AddressPersistentLocalId>();
            fakeAddressConsumerContext.AddressConsumerItems.Add(
                new AddressConsumerItem(
                    addressPersistentLocalId1,
                    AddressStatus.Parse(invalidStatus),
                    "geometryMethod",
                    "geometrySpec",
                    GeometryHelpers.ValidPoint1InPolgyon2
                ));
            fakeAddressConsumerContext.SaveChanges();

            var result = fakeAddressConsumerContext.FindAddressesWithinGeometry(GeometryHelpers.ValidPolygon3);

            result.Should().BeEmpty();

        }
    }
}
