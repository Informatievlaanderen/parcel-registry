namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using System;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using WhenImportingSubaddressFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithAddresses : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelWithAddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WithDeleteAndInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var deleteCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId)
                .WithHouseNumberId(command.HouseNumberId)
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId)),
                    new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                    command.ToLegacyEvent()));
        }

        //[Fact]
        //public void WithDifferentHouseNumberId_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var oldHouseNumberId = new CrabHouseNumberId(-1);
        //    var oldCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
        //        .WithModification(CrabModification.Insert)
        //        .WithHouseNumberId(oldHouseNumberId);

        //    var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
        //        .WithModification(CrabModification.Correction)
        //        .WithTerrainObjectHouseNumberId(oldCommand.TerrainObjectHouseNumberId);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelAddressWasAttached>()
        //                .WithAddressId(AddressId.CreateFor(oldHouseNumberId)),
        //            oldCommand.ToLegacyEvent())
        //        .When(command)
        //        .Then(new[]
        //        {
        //            new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, AddressId.CreateFor(oldHouseNumberId))),
        //            new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId))),
        //            new Fact(_parcelId, command.ToLegacyEvent()),
        //            new Fact(_snapshotId,
        //                SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
        //                    .WithLastModificationBasedOnCrab(Modification.Update)
        //                    .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId)})
        //                    .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
        //                    {
        //                        { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
        //                    })
        //                    .Build(5, EventSerializerSettings))
        //        }));
        //}

        [Fact]
        public void AddAnotherTerrainObjectHouseNumber()
        {
            var oldHouseNumberId = new CrabHouseNumberId(-1);
            var oldCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(oldHouseNumberId);

            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var firstHouseNrCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress2.SubaddressId)),
                    subaddress2.ToLegacyEvent())
                .When(secondCommand)
                .Then(_parcelId,
                    secondCommand.ToLegacyEvent()));
        }

        [Fact]
        public void AddAlreadyExistingSubaddressForOtherTerrainObjectHouseNr()
        {
            var firstHouseNrCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var firstHouseNrCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(new CrabHouseNumberId(-1))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(new CrabHouseNumberId(-1))
                .WithModification(CrabModification.Insert);

            var firstChangedCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithTerrainObjectHouseNumberId(firstHouseNrCommand.TerrainObjectHouseNumberId)
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var firstHouseNrCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            var removeSubaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithSubaddressId(subaddress2.SubaddressId)
                .WithModification(CrabModification.Delete)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var firstHouseNrCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            var removeFirstHouseNr = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var firstHouseNrCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithModification(CrabModification.Insert)
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId);

            var secondCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(firstHouseNrCommand.HouseNumberId)
                .WithModification(CrabModification.Correction);

            var removeSecondHouseNr = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithHouseNumberId(secondCommand.HouseNumberId)
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(firstHouseNrCommand.HouseNumberId)),
                    firstHouseNrCommand.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(subaddress1.SubaddressId)),
                    subaddress1.ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
