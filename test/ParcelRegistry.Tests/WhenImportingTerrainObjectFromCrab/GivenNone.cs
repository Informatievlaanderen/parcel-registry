namespace ParcelRegistry.Tests.WhenImportingTerrainObjectFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using global::AutoFixture;
    using Xunit.Abstractions;

    public class GivenNone : ParcelRegistryTest
    {
        private readonly Fixture _fixture;

        public GivenNone(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
        }

        //[Fact]
        //public void ThenIsRegistered_WithSnapshot()
        //{
        //    _fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

        //    var command = _fixture.Create<ImportTerrainObjectFromCrab>()
        //        .WithLifetime(new CrabLifetime(null, null))
        //        .WithModification(CrabModification.Insert);

        //    var parcelId = new ParcelId(command.CaPaKey.CreateDeterministicId());
        //    var snapshotId = GetSnapshotIdentifier(parcelId);

        //    Assert(new Scenario()
        //        .GivenNone()
        //        .When(command)
        //        .Then(new []
        //        {
        //            new Fact(parcelId, new ParcelWasRegistered(parcelId, command.CaPaKey)),
        //            new Fact(parcelId, new ParcelWasRealized(parcelId)),
        //            new Fact(parcelId, command.ToLegacyEvent()),
        //            new Fact(snapshotId,
        //                SnapshotBuilder.CreateDefaultSnapshot(parcelId)
        //                    .WithParcelStatus(ParcelStatus.Realized)
        //                    .Build(2, EventSerializerSettings))
        //        }));
        //}
    }
}
