namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
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

        //[Fact]
        //public void WhenLifetimeIsFinite_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>())
        //        .When(command)
        //        .Then(new []
        //        {
        //            new Fact(_parcelId, new ParcelWasRetired(_parcelId)),
        //            new Fact(_parcelId, command.ToLegacyEvent()),
        //            new Fact(_snapshotId,
        //                SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
        //                    .WithParcelStatus(ParcelStatus.Retired)
        //                    .Build(2, EventSerializerSettings))
        //        }));
        //}

        [Fact]
        public void WhenLifetimeIsFinite_BasedOnSnapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId, _fixture.Create<ParcelWasRegistered>())
                .Given(_snapshotId, SnapshotBuilder.CreateDefaultSnapshot(_parcelId).Build(0, EventSerializerSettings))
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelWasRetired(_parcelId)),
                    new Fact(_parcelId, command.ToLegacyEvent())
                }));
        }

        [Fact]
        public void WhenLifetimeIsFiniteAndCorrection()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasCorrectedToRetired(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsFiniteWithAlreadyRetired()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsFiniteWithAlreadyCorrectedToRetired()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasCorrectedToRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsInfinite()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasRealized(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsInfiniteAndCorrection()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasCorrectedToRealized(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsInfiniteWhenAlreadyRealized()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenInfifetimeIsInfiniteWhenAlreadyCorrectedToRealized()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasCorrectedToRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        //[Fact]
        //public void WhenModificationDelete_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithModification(CrabModification.Delete);

        //    Assert(new Scenario()
        //        .Given(_parcelId,
        //            _fixture.Create<ParcelWasRegistered>())
        //        .When(command)
        //        .Then(new []
        //        {
        //            new Fact(_parcelId, new ParcelWasRemoved(_parcelId)),
        //            new Fact(_parcelId, command.ToLegacyEvent()),
        //            new Fact(_snapshotId,
        //                SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
        //                    .WithIsRemoved(true)
        //                    .WithLastModificationBasedOnCrab(Modification.Delete)
        //                    .Build(2, EventSerializerSettings))
        //        }));
        //}
    }
}
