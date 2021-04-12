namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using global::AutoFixture;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNone : ParcelRegistryTest
    {
        public GivenNone(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenIsRegistered_WithSnapshot()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(null, null))
                .WithModification(CrabModification.Insert);

            var parcelId = new ParcelId(command.CaPaKey.CreateDeterministicId());
            var snapshotId = GetSnapshotIdentifier(parcelId);

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new []
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
