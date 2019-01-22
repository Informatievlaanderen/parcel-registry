namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using WhenImportingSubaddressFromCrab;
    using WhenImportingTerrainObjectHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithAddresses : ParcelRegistryTest
    {
        private readonly Fixture _fixture;
        private readonly ParcelId _parcelId;

        public GivenParcelWithAddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
        }

        [Fact]
        public void WithDeleteAndInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndFiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent()));
        }

        //TODO: Ruben: historeren terreinobjecthuisnr => adres relaties weg zoals bij historeren gebouweenheid? case 4 zegt van niet
    }
}
