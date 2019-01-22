namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcel : ParcelRegistryTest
    {
        private readonly Fixture _fixture;
        private readonly ParcelId _parcelId;

        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            Assert(new Scenario()
                .Given(_parcelId,
                        _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                        new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                        command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetimeWithAddressAlreadyAdded()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }


        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithNoAddresses()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithNoAddresses()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithAddress()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    command.ToLegacyEvent()));
        }
    }
}
