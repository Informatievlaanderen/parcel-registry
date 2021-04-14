namespace ParcelRegistry.Tests.WhenImportingSubaddressFromCrab
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using Parcel.Events.Crab;
    using SnapshotTests;
    using WhenImportingTerrainObjectHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithRetiredHouseNumber : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcelWithRetiredHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WhenLifetimeIsFinite_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var terrainObjectHouseNumberWasImportedFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRealized>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    terrainObjectHouseNumberWasImportedFromCrab,
                    Fixture.Create<ParcelAddressWasDetached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .WithLifetime(command.Lifetime)
                        .WithTerrainObjectHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId))
                        .WithModification(CrabModification.Historize)
                        .ToLegacyEvent())
                .When(command)
                .Then(new []
                {
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder
                            .CreateDefaultSnapshot(_parcelId)
                            .WithParcelStatus(ParcelStatus.Realized)
                            .WithAddressIds(new List<AddressId>())
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab> { command.ToLegacyEvent() })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(6, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenLifetimeIsFinite_BaseOnSnapshot()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var terrainObjectHouseNumberWasImportedFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId, Fixture.Create<ParcelWasRegistered>())
                .Given(_snapshotId,
                    SnapshotBuilder
                            .CreateDefaultSnapshot(_parcelId)
                            .WithParcelStatus(ParcelStatus.Retired)
                            .WithAddressIds(new List<AddressId>())
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab> { command.ToLegacyEvent() })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(5, EventSerializerSettings))
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, command.ToLegacyEvent()),
                }));
        }
    }
}
