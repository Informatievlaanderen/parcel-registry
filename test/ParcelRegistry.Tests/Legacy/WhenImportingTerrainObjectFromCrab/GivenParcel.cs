namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectFromCrab
{
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
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

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
        public void WhenLifetimeIsFinite_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(new []
                {
                    new Fact(_parcelId, new ParcelWasRetired(_parcelId)),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithParcelStatus(ParcelStatus.Retired)
                            .Build(2, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenLifetimeIsFinite_BasedOnSnapshot()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId, Fixture.Create<ParcelWasRegistered>())
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
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasCorrectedToRetired(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsFiniteWithAlreadyRetired()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsFiniteWithAlreadyCorrectedToRetired()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasCorrectedToRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsInfinite()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasRealized(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsInfiniteAndCorrection()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasCorrectedToRealized(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenLifetimeIsInfiniteWhenAlreadyRealized()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenInfifetimeIsInfiniteWhenAlreadyCorrectedToRealized()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasCorrectedToRetired>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenModificationDelete_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(new []
                {
                    new Fact(_parcelId, new ParcelWasRemoved(_parcelId)),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithIsRemoved(true)
                            .WithLastModificationBasedOnCrab(Modification.Delete)
                            .Build(2, EventSerializerSettings))
                }));
        }
    }
}
