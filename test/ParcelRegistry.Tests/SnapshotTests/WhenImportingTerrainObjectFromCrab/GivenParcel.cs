namespace ParcelRegistry.Tests.SnapshotTests.WhenImportingTerrainObjectFromCrab
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Newtonsoft.Json;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using Parcel.Events.Crab;
    using Tests.WhenImportingTerrainObjectFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcel : ParcelRegistrySnapshotTest
    {
        private readonly Fixture _fixture;
        private readonly ParcelId _parcelId;

        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
        }

        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";

        [Fact]
        public void WhenLifetimeIsFinite()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var snapshotId = GetSnapshotIdentifier(_parcelId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(snapshotId,

                    new SnapshotContainer
                    {
                        Info = { Position = 2, Type = nameof(ParcelSnapshot) },
                        Data = JsonConvert.SerializeObject(new ParcelSnapshot(
                                _parcelId,
                                ParcelStatus.Retired,
                                false,
                                Modification.Insert,
                                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(),
                                new List<AddressSubaddressWasImportedFromCrab>(),
                                new List<AddressId>()),
                            EventSerializerSettings)
                    }));
        }

        //[Fact]
        //public void WhenLifetimeIsFiniteAndCorrection()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
        //        .WithModification(CrabModification.Correction);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>())
        //        .When(command)
        //        .Then(_parcelId,
        //            new ParcelWasCorrectedToRetired(_parcelId),
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenLifetimeIsFiniteWithAlreadyRetired()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelWasRetired>())
        //        .When(command)
        //        .Then(_parcelId,
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenLifetimeIsFiniteWithAlreadyCorrectedToRetired()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelWasCorrectedToRetired>())
        //        .When(command)
        //        .Then(_parcelId,
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenLifetimeIsInfinite()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>())
        //        .When(command)
        //        .Then(_parcelId,
        //            new ParcelWasRealized(_parcelId),
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenLifetimeIsInfiniteAndCorrection()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
        //        .WithModification(CrabModification.Correction);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>())
        //        .When(command)
        //        .Then(_parcelId,
        //            new ParcelWasCorrectedToRealized(_parcelId),
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenLifetimeIsInfiniteWhenAlreadyRealized()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelWasRetired>())
        //        .When(command)
        //        .Then(_parcelId,
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenInfifetimeIsInfiniteWhenAlreadyCorrectedToRealized()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>(),
        //            _fixture.Create<ParcelWasCorrectedToRetired>())
        //        .When(command)
        //        .Then(_parcelId,
        //            command.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenModificationDelete()
        //{
        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithModification(CrabModification.Delete);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>())
        //        .When(command)
        //        .Then(_parcelId,
        //            new ParcelWasRemoved(_parcelId),
        //            command.ToLegacyEvent()));
        //}
    }
}
