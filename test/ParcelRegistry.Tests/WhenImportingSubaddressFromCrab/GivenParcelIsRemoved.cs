namespace ParcelRegistry.Tests.WhenImportingSubaddressFromCrab
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using WhenImportingTerrainObjectHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelIsRemoved : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void AddTerrainObjectHouseNumber()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent(),
                    _fixture.Create<ParcelWasRemoved>()
                )
                .When(command)
                .Throws(new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}")));
        }

        [Fact]
        public void AddTerrainObjectHouseNumber_BasedOnSnapshot()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            var terrainObjectHouseNumberWasImportedFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId, _fixture.Create<ParcelWasRegistered>())
                .Given(_snapshotId,
                    SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                        .WithAddressIds(new[] { AddressId.CreateFor(command.HouseNumberId) })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>()
                        {
                            { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.HouseNumberId) }
                        })
                        .WithIsRemoved(true)
                        .Build(3, EventSerializerSettings)
                )
                .When(command)
                .Throws(new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}")));
        }
    }
}
