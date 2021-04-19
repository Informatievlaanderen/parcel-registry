namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcel : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            Assert(new Scenario()
                .Given(_parcelId,
                        _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                        new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId)),
                        command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndInfiniteLifetimeWithAddressAlreadyAdded()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithNoAddresses()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        //[Fact]
        //public void WhenDeleteAndInfiniteLifetimeWithAddress_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
        //        .WithModification(CrabModification.Insert);

        //    var deleteCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
        //        .WithTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId)
        //        .WithHouseNumberId(command.HouseNumberId)
        //        .WithModification(CrabModification.Delete);

        //    var addressId = AddressId.CreateFor(command.HouseNumberId);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelAddressWasAttached>()
        //                .WithAddressId(addressId),
        //            command.ToLegacyEvent())
        //        .When(deleteCommand)
        //        .Then(new[]
        //            {
        //                new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, addressId)),
        //                new Fact(_parcelId, deleteCommand.ToLegacyEvent()),
        //                new Fact(_snapshotId,
        //                    SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
        //                        .WithLastModificationBasedOnCrab(Modification.Update)
        //                        .Build(4, EventSerializerSettings))
        //            }));
        //}

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress_BasedOnSnapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            var deleteCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId)
                .WithHouseNumberId(command.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
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
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithAddress()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId),
                    command.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    command.ToLegacyEvent()));
        }
    }
}
