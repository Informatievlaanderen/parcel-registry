namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectHouseNumberFromCrab
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
    using WhenImportingSubaddressFromCrab;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

    public class GivenParcelWithBufferedSubaddresses : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcelWithBufferedSubaddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(new []
                {
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId))),
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId))),
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId))),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithVbrCaPaKey(command.CaPaKey)
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab>
                            {
                                subaddress1.ToLegacyEvent(),
                                subaddress2.ToLegacyEvent()
                            })
                            .WithAddressIds(new List<AddressId>
                            {
                                AddressId.CreateFor(command.HouseNumberId),
                                AddressId.CreateFor(subaddress1.SubaddressId),
                                AddressId.CreateFor(subaddress2.SubaddressId)
                            })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(6, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithDeleteAndInfiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithDeleteAndFiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetimeWhenOneSubaddressHasFiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetimeWhenOneSubaddressHasDelete()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete)
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetimeWhereOneSubaddressHasOtherHouseNumberId()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetimeWhenOneSubaddressHasAlreadyBeenAdded()
        {
            //TODO: Check Ruben if possible to add subaddress twice (via different terrain object hnr)

            //var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
            //    .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            //var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
            //    .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
            //    .WithHouseNumberId(command.HouseNumberId);

            //var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
            //    .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
            //    .WithHouseNumberId(command.HouseNumberId);

            //Assert(new Scenario()
            //    .Given(_parcelId,
            //        _fixture.Create<ParcelWasRegistered>(),
            //        subaddress1.ToLegacyEvent(),
            //        subaddress2.ToLegacyEvent())
            //    .When(command)
            //    .Then(_parcelId,
            //        new ParcelVersionWasIncreased(_parcelId, new Version(1)),
            //        new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
            //        new ParcelVersionWasIncreased(_parcelId, new Version(2)),
            //        new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId)),
            //        new ParcelVersionWasIncreased(_parcelId, new Version(3)),
            //        new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
            //        command.ToLegacyEvent()));
        }
    }
}
