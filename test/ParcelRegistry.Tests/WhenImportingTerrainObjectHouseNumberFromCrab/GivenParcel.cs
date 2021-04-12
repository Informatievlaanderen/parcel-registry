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
        private readonly ParcelId _parcelId;

        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            Assert(new Scenario()
                .Given(_parcelId,
                        Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                        new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                        command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetimeWithAddressAlreadyAdded()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }


        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithNoAddresses()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            var deleteCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId)
                .WithHouseNumberId(command.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId),
                    command.ToLegacyEvent())
                .When(deleteCommand)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    deleteCommand.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithNoAddresses()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithAddress()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId),
                    command.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    command.ToLegacyEvent()));
        }
    }
}
