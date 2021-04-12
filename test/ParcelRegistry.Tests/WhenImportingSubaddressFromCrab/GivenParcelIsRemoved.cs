namespace ParcelRegistry.Tests.WhenImportingSubaddressFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using WhenImportingTerrainObjectHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelIsRemoved : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;

        public GivenParcelIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
        }

        [Fact]
        public void AddTerrainObjectHouseNumber()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent(),
                    Fixture.Create<ParcelWasRemoved>()
                )
                .When(command)
                .Throws(new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}")));
        }
    }
}
