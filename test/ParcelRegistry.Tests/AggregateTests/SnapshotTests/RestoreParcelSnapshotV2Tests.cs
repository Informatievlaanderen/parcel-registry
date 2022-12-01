namespace ParcelRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Parcel;
    using Parcel.Events;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using Autofac;

    public class RestoreParcelSnapshotV2Tests : ParcelRegistryTest
    {
        private readonly Parcel _sut;
        private readonly ParcelSnapshotV2 _parcelSnapshotV2;

        public RestoreParcelSnapshotV2Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());

            _sut = new ParcelFactory(IntervalStrategy.Default, Container.Resolve<IAddresses>()).Create();
            _parcelSnapshotV2 = new ParcelSnapshotV2(
                Fixture.Create<ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                Fixture.Create<ParcelStatus>(),
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                Fixture.Create<string>(),
                Fixture.Create<ProvenanceData>());

            _sut.Initialize(new List<object> { _parcelSnapshotV2 });
        }

        [Fact]
        public void ThenAggregateParcelStateIsExpected()
        {
            _sut.ParcelId.Should().Be(new ParcelId(_parcelSnapshotV2.ParcelId));
            _sut.CaPaKey.Should().Be(new VbrCaPaKey(_parcelSnapshotV2.CaPaKey));
            _sut.ParcelStatus.Should().Be(ParcelStatus.Parse(_parcelSnapshotV2.ParcelStatus));
            _sut.IsRemoved.Should().Be(_parcelSnapshotV2.IsRemoved);
            _sut.AddressPersistentLocalIds.Select(x => (int) x).Should()
                .BeEquivalentTo(_parcelSnapshotV2.AddressPersistentLocalIds);
            ((decimal?)_sut.XCoordinate!).Should().Be(_parcelSnapshotV2.XCoordinate);
            ((decimal?)_sut.YCoordinate!).Should().Be(_parcelSnapshotV2.YCoordinate);
            _sut.LastEventHash.Should().Be(_parcelSnapshotV2.LastEventHash);
            _sut.LastProvenanceData.Should().Be(_parcelSnapshotV2.LastProvenanceData);
        }
    }
}
