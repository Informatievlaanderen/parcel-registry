namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Extensions;
    using Parcel.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using Fixtures;
    using NetTopologySuite.IO;
    using Parcel;
    using Projections.Legacy.ParcelSyndication;
    using Xunit;

    public class ParcelSyndicationV2Tests : ParcelLegacyProjectionTest<ParcelSyndicationProjections>
    {
        private readonly Fixture? _fixture;

        public ParcelSyndicationV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithParcelStatus());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            _fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public async Task WhenParcelWasMigrated()
        {
            const long position = 1L;
            var message = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasMigrated) },
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(message, metadata)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem!.CaPaKey.Should().Be(message.CaPaKey);
                    parcelSyndicationItem.Status.Should().Be(ParcelRegistry.Legacy.ParcelStatus.Parse(message.ParcelStatus));
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().BeEquivalentTo(message.AddressPersistentLocalIds);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelWasMigrated));
                    parcelSyndicationItem.ExtendedWkbGeometry.Should().BeEquivalentTo(message.ExtendedWkbGeometry.ToByteArray());
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(message.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(message.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelWasImported()
        {
            const long position = 1L;
            var message = _fixture.Create<ParcelWasImported>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasImported) },
            };

            await Sut
                .Given(new Envelope<ParcelWasImported>(new Envelope(message, metadata)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem!.CaPaKey.Should().Be(message.CaPaKey);
                    parcelSyndicationItem.Status.Should().Be(ParcelRegistry.Legacy.ParcelStatus.Realized);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelWasImported));
                    parcelSyndicationItem.ExtendedWkbGeometry.Should().BeEquivalentTo(message.ExtendedWkbGeometry.ToByteArray());
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(message.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(message.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelWasRetiredV2()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var parcelWasRetiredV2 = _fixture.Create<ParcelWasRetiredV2>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasImported, 1L),
                    CreateEnvelope(parcelWasRetiredV2, 2L))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(2L);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem!.CaPaKey.Should().Be(parcelWasRetiredV2.CaPaKey);
                    parcelSyndicationItem.Status.Should().Be(ParcelRegistry.Legacy.ParcelStatus.Retired);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelWasRetiredV2));
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.LastChangedOn.Should().Be(parcelWasRetiredV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelGeometryWasChanged()
        {
            var parcelId = _fixture.Create<ParcelId>();
            var caPaKey = _fixture.Create<VbrCaPaKey>();

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var parcelGeometryWasChanged = new ParcelGeometryWasChanged(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());

            await Sut
                .Given(
                    CreateEnvelope(parcelWasImported, 1L),
                    CreateEnvelope(parcelGeometryWasChanged, 2L))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(2L);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelGeometryWasChanged));
                    parcelSyndicationItem.ExtendedWkbGeometry.Should().BeEquivalentTo(GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry().ToString().ToByteArray());
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasImported.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(parcelGeometryWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedFromRetiredToRealized()
        {
            var parcelId = _fixture.Create<ParcelId>();
            var caPaKey = _fixture.Create<VbrCaPaKey>();

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var @event = new ParcelWasCorrectedFromRetiredToRealized(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());

            await Sut
                .Given(
                    CreateEnvelope(parcelWasImported, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(2L);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelWasCorrectedFromRetiredToRealized));
                    parcelSyndicationItem.ExtendedWkbGeometry.Should().BeEquivalentTo(GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry().ToString().ToByteArray());
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.Status.Should().Be(ParcelRegistry.Legacy.ParcelStatus.Realized);
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasImported.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(@event.Provenance.Timestamp);
                });
        }

        private Envelope<TEvent> CreateEnvelope<TEvent>(TEvent @event, long position)
            where TEvent : IMessage, IHaveHash
        {
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());
            return new Envelope<TEvent>(new Envelope(@event, new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey,typeof(TEvent).Name }
            }));
        }

        [Fact]
        public async Task WhenParcelAddressWasAttachedV2()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, 1L },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasMigrated) },
            };

            const long position = 2L;
            var addressWasAttached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelAddressWasAttachedV2) },
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(addressWasAttached, metadata2)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem.AddressPersistentLocalIds.Should()
                        .BeEquivalentTo(parcelWasMigrated.AddressPersistentLocalIds.Concat(new[] { addressWasAttached.AddressPersistentLocalId }));
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelAddressWasAttachedV2));
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(addressWasAttached.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedV2()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, 1L },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasMigrated) },
            };

            const long position = 2L;
            var addressWasDetached = _fixture.Create<ParcelAddressWasDetachedV2>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelAddressWasDetachedV2) },
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasDetachedV2>(new Envelope(addressWasDetached, metadata2)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().NotContain(addressWasDetached.AddressPersistentLocalId);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelAddressWasDetachedV2));
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(addressWasDetached.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseAddressWasReaddressed()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);

            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>()
                .WithClearedAddresses()
                .WithAddress(previousAddressPersistentLocalId);
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, 1L },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasMigrated) },
            };

            const long position = 2L;
            var @event = _fixture.Create<ParcelAddressWasReplacedBecauseAddressWasReaddressed>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelAddressWasReplacedBecauseAddressWasReaddressed) },
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(new Envelope(@event, metadata2)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().NotContain(@event.PreviousAddressPersistentLocalId);
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().Contain(@event.NewAddressPersistentLocalId);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelAddressWasReplacedBecauseAddressWasReaddressed));
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(@event.Provenance.Timestamp);
                });
        }

        protected override ParcelSyndicationProjections CreateProjection()
            => new ParcelSyndicationProjections();
    }
}
