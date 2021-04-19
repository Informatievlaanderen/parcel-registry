namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNone_Snapshot : ParcelRegistrySnapshotTest
    {
        private readonly Fixture _fixture;

        public GivenNone_Snapshot(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenIsRegistered_WithSnapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(null, null))
                .WithModification(CrabModification.Insert);

            var parcelId = new ParcelId(command.CaPaKey.CreateDeterministicId());
            var snapshotId = GetSnapshotIdentifier(parcelId);

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new[]
                {
                    new Fact(parcelId, new ParcelWasRegistered(parcelId, command.CaPaKey)),
                    new Fact(parcelId, new ParcelWasRealized(parcelId)),
                    new Fact(parcelId, command.ToLegacyEvent()),
                    new Fact(snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(parcelId)
                            .WithParcelStatus(ParcelStatus.Realized)
                            .Build(2, EventSerializerSettings))
                }));
        }
    }
}
