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

    public class GivenParcelWithRetiredHouseNumber_Snapshot : ParcelRegistrySnapshotTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelWithRetiredHouseNumber_Snapshot(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WhenLifetimeIsFinite_WithSnapshot()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var terrainObjectHouseNumberWasImportedFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRealized>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    terrainObjectHouseNumberWasImportedFromCrab,
                    _fixture.Create<ParcelAddressWasDetached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .WithLifetime(command.Lifetime)
                        .WithTerrainObjectHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId))
                        .WithModification(CrabModification.Historize)
                        .ToLegacyEvent())
                .When(command)
                .Then(new[]
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
    }
}
