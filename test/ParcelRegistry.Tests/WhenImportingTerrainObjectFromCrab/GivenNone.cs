namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using global::AutoFixture;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNone : ParcelRegistryTest
    {
        private readonly Fixture _fixture;
        public GivenNone(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenIsRegistered()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(null, null))
                .WithModification(CrabModification.Insert);

            var parcelId = new ParcelId(command.CaPaKey.CreateDeterministicId());
            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(parcelId,
                        new ParcelWasRegistered(parcelId, command.CaPaKey),
                        new ParcelWasRealized(parcelId),
                        command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenIsRegistered_Snapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(null, null))
                .WithModification(CrabModification.Insert);

            var parcelId = new ParcelId(command.CaPaKey.CreateDeterministicId());
            var snapshotId = GetSnapshotIdentifier(parcelId);

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(snapshotId,
                    SnapshotBuilder.CreateDefaultSnapshot(parcelId)
                        .WithParcelStatus(ParcelStatus.Realized)
                        .Build(2, EventSerializerSettings)));
        }
    }
}
