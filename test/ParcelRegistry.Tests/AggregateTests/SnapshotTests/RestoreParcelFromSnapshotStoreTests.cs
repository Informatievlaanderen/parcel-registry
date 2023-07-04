namespace ParcelRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class RestoreParcelFromSnapshotStoreV2Tests : ParcelRegistryTest
    {
        private readonly Parcel _sut;
        private readonly ParcelSnapshotV2 _parcelSnapshotV2;

        public RestoreParcelFromSnapshotStoreV2Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());

            _sut = new ParcelFactory(IntervalStrategy.Default, Container.Resolve<IAddresses>()).Create();
            _parcelSnapshotV2 = new ParcelSnapshotV2(
                Fixture.Create<ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                Fixture.Create<ParcelStatus>(),
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                Fixture.Create<string>(),
                Fixture.Create<ProvenanceData>());

            var eventSerializer = Container.Resolve<EventSerializer>();
            var eventMapping = Container.Resolve<EventMapping>();
            var streamId = new ParcelStreamId(Fixture.Create<ParcelId>());
            Container.Resolve<ISnapshotStore>().SaveSnapshotAsync(streamId,
                new SnapshotContainer
                {
                    Data = eventSerializer.SerializeObject(_parcelSnapshotV2),
                    Info = new SnapshotInfo
                    {
                        StreamVersion = 1,
                        Type = eventMapping.GetEventName(_parcelSnapshotV2.GetType()),
                    }
                },
                CancellationToken.None);

            Container.Resolve<IStreamStore>().AppendToStream(new StreamId(streamId), ExpectedVersion.NoStream, Fixture.Create<NewStreamMessage>());

            _sut = Container.Resolve<IParcels>().GetAsync(streamId, CancellationToken.None).GetAwaiter().GetResult();
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
            _sut.Geometry.Should().Be(new ExtendedWkbGeometry(_parcelSnapshotV2.ExtendedWkbGeometry));
            _sut.LastEventHash.Should().Be(_parcelSnapshotV2.LastEventHash);
            _sut.LastProvenanceData.Should().BeEquivalentTo(_parcelSnapshotV2.LastProvenanceData);
        }
    }
}
