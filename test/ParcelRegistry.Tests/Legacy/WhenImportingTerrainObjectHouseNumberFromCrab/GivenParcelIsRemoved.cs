namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Fixtures;
    using global::AutoFixture;
    using NodaTime;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands.Crab;
    using ParcelRegistry.Legacy.Events;
    using ParcelRegistry.Legacy.Exceptions;
    using Legacy;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

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
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRemoved>()
                )
                .When(command)
                .Throws(new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}")));
        }
    }
}
