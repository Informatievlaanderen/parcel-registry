namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using WhenImportingSubaddressFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithBufferedSubaddresses : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelWithBufferedSubaddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

        //    var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

        //    var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
        //        .WithHouseNumberId(command.HouseNumberId);

        //    var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
        //        .WithHouseNumberId(command.HouseNumberId);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            subaddress1.ToLegacyEvent(),
        //            subaddress2.ToLegacyEvent())
        //        .When(command)
        //        .Then(new []
        //        {
        //            new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId))),
        //            new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId))),
        //            new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId))),
        //            new Fact(_parcelId, command.ToLegacyEvent()),
        //            new Fact(_snapshotId,
        //                SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
        //                    .WithLastModificationBasedOnCrab(Modification.Update)
        //                    .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab>
        //                    {
        //                        subaddress1.ToLegacyEvent(),
        //                        subaddress2.ToLegacyEvent()
        //                    })
        //                    .WithAddressIds(new List<AddressId>
        //                    {
        //                        AddressId.CreateFor(command.HouseNumberId),
        //                        AddressId.CreateFor(subaddress1.SubaddressId),
        //                        AddressId.CreateFor(subaddress2.SubaddressId)
        //                    })
        //                    .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
        //                    {
        //                        { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
        //                    })
        //                    .Build(6, EventSerializerSettings))
        //        }));
        //}

        [Fact]
        public void WithDeleteAndInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithDeleteAndFiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete);

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetimeWhenOneSubaddressHasFiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
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
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Delete)
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
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
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
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
