namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using System;
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
    using Legacy;
    using SnapshotTests;
    using WhenImportingSubaddressFromCrab;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

    public class GivenParcelWithAddresses : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcelWithAddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WithDeleteAndInfiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var deleteCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId)
                .WithHouseNumberId(command.HouseNumberId)
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent())
                .When(deleteCommand)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    deleteCommand.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndFiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
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
        public void WithDifferentHouseNumberId_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var oldHouseNumberId = new CrabHouseNumberId(-1);
            var oldCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(oldHouseNumberId);

            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Correction)
                .WithTerrainObjectHouseNumberId(oldCommand.TerrainObjectHouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
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
                            .WithVbrCaPaKey(command.CaPaKey)
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId)})
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(5, EventSerializerSettings))
                }));
        }

        [Fact]
        public void AddAnotherTerrainObjectHouseNumber()
        {
            var oldHouseNumberId = new CrabHouseNumberId(-1);
            var oldCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(oldHouseNumberId);

            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(oldHouseNumberId)),
                    oldCommand.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void AddAnotherTerrainObjectHouseNumberWithSameHouseNumber()
        {
            var firstHouseNrCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent())
                .When(secondCommand)
                .Then(_parcelId,
                    secondCommand.ToLegacyEvent()));
        }

        [Fact]
        public void AddAlreadyExistingSubaddressForOtherTerrainObjectHouseNr()
        {
            var firstHouseNrCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent(),
                    secondCommand.ToLegacyEvent())
                .When(subaddress1)
                .Then(_parcelId,
                    subaddress1.ToLegacyEvent()));
        }

        [Fact]
        public void ChangeAddedTerrainObjectHouseNumberToNewHouseNumber()
        {
            var firstHouseNrCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(new CrabHouseNumberId(-1))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(new CrabHouseNumberId(-1))
                .WithModification(CrabModification.Insert);

            var firstChangedCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithTerrainObjectHouseNumberId(firstHouseNrCommand.TerrainObjectHouseNumberId)
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent(),
                    secondCommand.ToLegacyEvent())
                .When(firstChangedCommand)
                .Then(_parcelId,
                    new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(firstChangedCommand.HouseNumberId)),
                    firstChangedCommand.ToLegacyEvent()));
        }

        [Fact]
        public void RemoveSubaddressCoupledToTwoTerrainObjectHouseNumbers()
        {
            var firstHouseNrCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            var removeSubaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithSubaddressId(subaddress2.SubaddressId)
                .WithModification(CrabModification.Delete)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent(),
                    secondCommand.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(removeSubaddress2)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(removeSubaddress2.SubaddressId)),
                    removeSubaddress2.ToLegacyEvent()));
        }

        [Fact]
        public void RemoveOneHouseNumberCoupledToTwoSubaddresses()
        {
            var firstHouseNrCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            var removeFirstHouseNr = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent(),
                    secondCommand.ToLegacyEvent(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(removeFirstHouseNr)
                .Then(_parcelId,
                    removeFirstHouseNr.ToLegacyEvent()));
        }

        [Fact]
        public void RemoveOneHouseNumberCoupledToTwoSubaddressesOtherHouseNumber()
        {
            var firstHouseNrCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            var removeSecondHouseNr = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(secondCommand.HouseNumberId)
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent(),
                    secondCommand.ToLegacyEvent(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(removeSecondHouseNr)
                .Then(_parcelId,
                    removeSecondHouseNr.ToLegacyEvent()));
        }
    }
}
