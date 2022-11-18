namespace ParcelRegistry.Tests.Legacy.WhenImportingSubaddressFromCrab
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using global::AutoFixture;
    using NodaTime;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands.Crab;
    using ParcelRegistry.Legacy.Events;
    using ParcelRegistry.Legacy.Events.Crab;
    using Legacy;
    using SnapshotTests;
    using WhenImportingTerrainObjectHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

    [Collection("BasedOnSnapshotCollection")]
    public class GivenParcelWithAddressByHouseNumber : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcelWithAddressByHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetime_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.SubaddressId);

            var terrainObjectHouseNumberWasImportedFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRealized>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    terrainObjectHouseNumberWasImportedFromCrab)
                .When(command)
                .Then(new []
                {
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, addressId)),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder
                            .CreateDefaultSnapshot(_parcelId)
                            .WithVbrCaPaKey(command.CaPaKey)
                            .WithParcelStatus(ParcelStatus.Realized)
                            .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId), addressId })
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab> { command.ToLegacyEvent() })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(5, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetime_BasedOnSnapshot()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.SubaddressId);

            var terrainObjectHouseNumberWasImportedFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId, Fixture.Create<ParcelWasRegistered>())
                .Given(_snapshotId,
                    SnapshotBuilder
                        .CreateDefaultSnapshot(_parcelId)
                        .WithParcelStatus(ParcelStatus.Realized)
                        .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId) })
                        .WithLastModificationBasedOnCrab(Modification.Update)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                        })
                        .Build(3, EventSerializerSettings))
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, addressId)),
                    new Fact(_parcelId, command.ToLegacyEvent())
                }));
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetimeWithAddressAlreadyAdded()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.SubaddressId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }


        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithNoAddress()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.SubaddressId);
            var terrainObjectHouseNumberWasImportedFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    terrainObjectHouseNumberWasImportedFromCrab,
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(new[] {
                    new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, addressId)),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder
                            .CreateDefaultSnapshot(_parcelId)
                            .WithVbrCaPaKey(command.CaPaKey)
                            .WithParcelStatus(null)
                            .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId) })
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab> { command.ToLegacyEvent() })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(5, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress_BasedOnSnapshot()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.SubaddressId);
            var terrainObjectHouseNumberWasImportedFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId, Fixture.Create<ParcelWasRegistered>())
                .Given(_snapshotId,
                    SnapshotBuilder
                        .CreateDefaultSnapshot(_parcelId)
                        .WithParcelStatus(null)
                        .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId), AddressId.CreateFor(command.SubaddressId) })
                        .WithLastModificationBasedOnCrab(Modification.Update)
                        .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab>
                        {
                            command
                                .WithModification(CrabModification.Insert)
                                .ToLegacyEvent()
                        })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                        })
                        .Build(5, EventSerializerSettings))
                .When(command)
                .Then(new[] {
                    new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, addressId)),
                    new Fact(_parcelId, command.ToLegacyEvent())
                }));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithNoAddress()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>();

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithAddress()
        {
            var command = Fixture.Create<ImportSubaddressFromCrab>();

            var addressId = AddressId.CreateFor(command.SubaddressId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    command.ToLegacyEvent()));
        }
    }
}
