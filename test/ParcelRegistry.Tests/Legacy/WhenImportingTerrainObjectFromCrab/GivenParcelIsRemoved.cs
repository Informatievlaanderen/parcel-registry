namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Fixtures;
    using global::AutoFixture;
    using NodaTime;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands.Crab;
    using ParcelRegistry.Legacy.Events;
    using ParcelRegistry.Legacy.Exceptions;
    using Legacy;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

    [Collection("BasedOnSnapshotCollection")]
    public class GivenParcelIsRemoved : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcelIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
            _parcelId = Fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Theory]
        [InlineData(CrabModification.Correction)]
        [InlineData(CrabModification.Delete)]
        [InlineData(CrabModification.Historize)]
        public void WhenModificationIsNotInsertThenExceptionIsThrown(CrabModification modification)
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(modification);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRemoved>())
                .When(command)
                .Throws(new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}")));
        }

        [Fact]
        public void WhenModificationIsInsertThenParcelWasRecovered_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRemoved>())
                .When(command)
                .Then(new []
                {
                    new Fact(_parcelId, new ParcelWasRecovered(_parcelId)),
                    new Fact(_parcelId, new ParcelWasRealized(_parcelId)),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithVbrCaPaKey(command.CaPaKey)
                            .WithParcelStatus(ParcelStatus.Realized)
                            .WithCoordinates(command.XCoordinate, command.YCoordinate)
                            .Build(4, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenModificationIsInsertThenParcelWasRecovered_BasedOnSnapshot()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId, Fixture.Create<ParcelWasRegistered>())
                .Given(_snapshotId,
                    SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                        .WithIsRemoved(true)
                        .Build(0, EventSerializerSettings))
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelWasRecovered(_parcelId)),
                    new Fact(_parcelId, new ParcelWasRealized(_parcelId)),
                    new Fact(_parcelId, command.ToLegacyEvent())
                }));
        }
    }
}
