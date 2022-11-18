namespace ParcelRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using FluentAssertions;
    using ParcelRegistry.Parcel;
    using ParcelRegistry.Parcel.Events;
    using ParcelRegistry.Tests.Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class RestoreParcelSnapshotV2Tests : ParcelRegistryTest
    {
        private readonly Parcel _sut;
        private readonly ParcelSnapshotV2 _parcelSnapshotV2;

        public RestoreParcelSnapshotV2Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());

            _sut = new ParcelFactory(IntervalStrategy.Default).Create();
            _parcelSnapshotV2 = Fixture.Create<ParcelSnapshotV2>();
            _sut.Initialize(new List<object> { _parcelSnapshotV2 });
        }

        [Fact]
        public void ThenAggregateParcelStateIsExpected()
        {
            _sut.ParcelId.Should().Be(new ParcelId(_parcelSnapshotV2.ParcelId));
            _sut.ParcelStatus.Should().Be(ParcelStatus.Parse(_parcelSnapshotV2.ParcelStatus));
            _sut.IsRemoved.Should().Be(_parcelSnapshotV2.IsRemoved);
            _sut.LastEventHash.Should().Be(_parcelSnapshotV2.LastEventHash);
            _sut.LastProvenanceData.Should().Be(_parcelSnapshotV2.LastProvenanceData);
        }
    }
}
