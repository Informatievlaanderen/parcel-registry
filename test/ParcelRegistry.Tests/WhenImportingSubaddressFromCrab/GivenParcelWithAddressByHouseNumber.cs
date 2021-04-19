namespace ParcelRegistry.Tests.WhenImportingSubaddressFromCrab
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
    using Parcel.Events.Crab;
    using SnapshotTests;
    using WhenImportingTerrainObjectHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithAddressByHouseNumber : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelWithAddressByHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        //[Fact]
        //public void WithNoDeleteAndInfiniteLifetime_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var command = _fixture.Create<ImportSubaddressFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

        //    var addressId = AddressId.CreateFor(command.SubaddressId);

        //    var terrainObjectHouseNumberWasImportedFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithHouseNumberId(command.HouseNumberId)
        //        .ToLegacyEvent();

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelWasRealized>(),
        //            _fixture.Create<ParcelAddressWasAttached>()
        //                .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
        //            terrainObjectHouseNumberWasImportedFromCrab)
        //        .When(command)
        //        .Then(new []
        //        {
        //            new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, addressId)),
        //            new Fact(_parcelId, command.ToLegacyEvent()),
        //            new Fact(_snapshotId,
        //                SnapshotBuilder
        //                    .CreateDefaultSnapshot(_parcelId)
        //                    .WithParcelStatus(ParcelStatus.Realized)
        //                    .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId), addressId })
        //                    .WithLastModificationBasedOnCrab(Modification.Update)
        //                    .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab> { command.ToLegacyEvent() })
        //                    .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
        //                    {
        //                        { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
        //                    })
        //                    .Build(5, EventSerializerSettings))
        //        }));
        //}

        [Fact]
        public void WithNoDeleteAndInfiniteLifetime_BasedOnSnapshot()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.SubaddressId);

            var terrainObjectHouseNumberWasImportedFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId, _fixture.Create<ParcelWasRegistered>())
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
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var addressId = AddressId.CreateFor(command.SubaddressId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }


        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithNoAddress()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        //[Fact]
        //public void WhenDeleteAndInfiniteLifetimeWithAddress_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var command = _fixture.Create<ImportSubaddressFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
        //        .WithModification(CrabModification.Delete);

        //    var addressId = AddressId.CreateFor(command.SubaddressId);
        //    var terrainObjectHouseNumberWasImportedFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithHouseNumberId(command.HouseNumberId)
        //        .ToLegacyEvent();

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelAddressWasAttached>()
        //                .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
        //            terrainObjectHouseNumberWasImportedFromCrab,
        //            _fixture.Create<ParcelAddressWasAttached>()
        //                .WithAddressId(addressId))
        //        .When(command)
        //        .Then(new[] {
        //            new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, addressId)),
        //            new Fact(_parcelId, command.ToLegacyEvent()),
        //            new Fact(_snapshotId,
        //                SnapshotBuilder
        //                    .CreateDefaultSnapshot(_parcelId)
        //                    .WithParcelStatus(null)
        //                    .WithAddressIds(new List<AddressId> { AddressId.CreateFor(command.HouseNumberId) })
        //                    .WithLastModificationBasedOnCrab(Modification.Update)
        //                    .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab> { command.ToLegacyEvent() })
        //                    .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
        //                    {
        //                        { new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberWasImportedFromCrab.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
        //                    })
        //                    .Build(5, EventSerializerSettings))
        //            }));
        //}

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress_BasedOnSnapshot()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.SubaddressId);
            var terrainObjectHouseNumberWasImportedFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(command.HouseNumberId)
                .ToLegacyEvent();

            Assert(new Scenario()
                .Given(_parcelId, _fixture.Create<ParcelWasRegistered>())
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
            var command = _fixture.Create<ImportSubaddressFromCrab>();

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoDeleteAndFiniteLifetimeWithAddress()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>();

            var addressId = AddressId.CreateFor(command.SubaddressId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(AddressId.CreateFor(command.HouseNumberId)),
                    _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                        .WithHouseNumberId(command.HouseNumberId)
                        .ToLegacyEvent(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId))
                .When(command)
                .Then(_parcelId,
                    new ParcelAddressWasDetached(_parcelId, addressId),
                    command.ToLegacyEvent()));
        }
    }
}
