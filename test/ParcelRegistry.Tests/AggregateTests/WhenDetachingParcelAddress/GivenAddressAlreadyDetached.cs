namespace ParcelRegistry.Tests.AggregateTests.WhenDetachingParcelAddress
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

            var command = new DetachAddressBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Current,
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .ThenNone());
        }
    }
}
