namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelIsRemoved_Snapshot : ParcelRegistrySnapshotTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelIsRemoved_Snapshot(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WhenModificationIsInsertThenParcelWasRecovered_WithSnapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRemoved>())
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelWasRecovered(_parcelId)),
                    new Fact(_parcelId, new ParcelWasRealized(_parcelId)),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithParcelStatus(ParcelStatus.Realized)
                            .Build(4, EventSerializerSettings))
                }));
        }
    }
}
