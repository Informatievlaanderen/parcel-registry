namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelIsRemoved : ParcelRegistryTest
    {
        private readonly Fixture _fixture;
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;

        public GivenParcelIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Theory]
        [InlineData(CrabModification.Correction)]
        [InlineData(CrabModification.Delete)]
        [InlineData(CrabModification.Historize)]
        public void WhenModificationIsNotInsertThenExceptionIsThrown(CrabModification modification)
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(modification);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRemoved>())
                .When(command)
                .Throws(new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}")));
        }

        [Fact]
        public void WhenModificationIsInsertThenParcelWasRecovered()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRemoved>())
                .When(command)
                .Then(_parcelId,
                    new ParcelWasRecovered(_parcelId),
                    new ParcelWasRealized(_parcelId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenModificationIsInsertThenParcelWasRecovered_Snapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelWasRemoved>())
                .When(command)
                .Then(_snapshotId,
                    SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                        .WithParcelStatus(ParcelStatus.Realized)
                        .Build(4, EventSerializerSettings)));
        }
    }
}
