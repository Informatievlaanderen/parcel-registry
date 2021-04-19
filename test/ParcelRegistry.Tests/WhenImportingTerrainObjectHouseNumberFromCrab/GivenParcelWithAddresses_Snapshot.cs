namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithAddresses_Snapshot : ParcelRegistrySnapshotTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelWithAddresses_Snapshot(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WithDifferentHouseNumberId_WithSnapshot()
        {
            var oldHouseNumberId = new CrabHouseNumberId(-1);
            var oldCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(oldHouseNumberId);

            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Correction)
                .WithTerrainObjectHouseNumberId(oldCommand.TerrainObjectHouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(oldHouseNumberId)),
                    oldCommand.ToLegacyEvent())
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(oldHouseNumberId))),
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId))),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId)})
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(5, EventSerializerSettings))
                }));
        }
    }
}
