namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectHouseNumberFromCrab
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
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands.Crab;
    using ParcelRegistry.Legacy.Events;
    using Legacy;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    [Collection("BasedOnSnapshotCollection")]
    public class GivenParcel : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
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
        public void WhenDeleteAndInfiniteLifetimeWithAddress_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

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
                .Then(new[]
                    {
                        new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, addressId)),
                        new Fact(_parcelId, deleteCommand.ToLegacyEvent()),
                        new Fact(_snapshotId,
                            SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                                .WithLastModificationBasedOnCrab(Modification.Update)
                                .Build(4, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress_BasedOnSnapshot()
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
                .Given(_snapshotId,
                    SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                        .WithLastModificationBasedOnCrab(Modification.Insert)
                        .WithAddressIds(new List<AddressId> { addressId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                        })
                        .Build(2, EventSerializerSettings))
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
