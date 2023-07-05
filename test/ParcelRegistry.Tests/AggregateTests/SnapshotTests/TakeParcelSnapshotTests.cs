namespace ParcelRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using FluentAssertions;
    using Parcel;
    using Parcel.Events;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class TakeParcelSnapshotTests : ParcelRegistryTest
    {
        public TakeParcelSnapshotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ParcelWasMigratedIsSavedInSnapshot()
        {
            var aggregate = new ParcelFactory(IntervalStrategy.Default, Container.Resolve<IAddresses>()).Create();

            var parcelWasMigrated = Fixture.Create<ParcelWasMigrated>();

            aggregate.Initialize(new List<object> { parcelWasMigrated });

            var snapshot = aggregate.TakeSnapshot();
            snapshot.Should().BeOfType<ParcelSnapshotV2>();

            var parcelSnapshotV2 = (ParcelSnapshotV2)snapshot;
            parcelSnapshotV2.ParcelId.Should().Be(parcelWasMigrated.ParcelId);
            parcelSnapshotV2.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
            parcelSnapshotV2.ParcelStatus.Should().Be(ParcelStatus.Parse(parcelSnapshotV2.ParcelStatus));
            parcelSnapshotV2.IsRemoved.Should().Be(parcelWasMigrated.IsRemoved);
            parcelSnapshotV2.AddressPersistentLocalIds.Should().BeEquivalentTo(parcelWasMigrated.AddressPersistentLocalIds);
            parcelSnapshotV2.ExtendedWkbGeometry.Should().Be(parcelWasMigrated.ExtendedWkbGeometry);
            parcelSnapshotV2.LastEventHash.Should().Be(parcelWasMigrated.GetHash());
            parcelSnapshotV2.LastProvenanceData.Should().Be(parcelWasMigrated.Provenance);
        }
    }
}
