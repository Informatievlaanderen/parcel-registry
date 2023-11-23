namespace ParcelRegistry.Tests.AggregateTests.WhenAttachingParcelAddress
{
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Consumer.Address;
    using Fixtures;
    using NetTopologySuite.Geometries;
    using Parcel;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressHasInvalidStatus : ParcelRegistryTest
    {
        public GivenAddressHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
            Fixture.Customize(new WithExtendedWkbGeometryPoint());
        }

        [Theory]
        [InlineData("Rejected")]
        [InlineData("Retired")]
        public void ThenThrowsAddressHasInvalidStatusException(string addressStatus)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(11111111);

            var command = new AttachAddressBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Parse(addressStatus),
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }
    }
}
