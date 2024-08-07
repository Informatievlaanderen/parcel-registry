namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using Parcel.Events;
    using Projections.Legacy.ParcelDetail;
    using Xunit;

    public partial class ParcelDetailItemV2Tests : ParcelLegacyProjectionTest<ParcelDetailProjections>
    {
        private readonly Fixture _fixture;

        public ParcelDetailItemV2Tests()
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
            var message = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() }
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(message, metadata)))
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var parcelDetailV2 = await context.ParcelDetails.FindAsync(message.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.CaPaKey.Should().Be(message.CaPaKey);
                    parcelDetailV2.Status.Should().Be(ParcelStatus.Parse(message.ParcelStatus));
                    parcelDetailV2.Removed.Should().Be(message.IsRemoved);
                    parcelDetailV2.Gml.Should().Be(geometry.ConvertToGml());
                    parcelDetailV2.GmlType.Should().Be("Polygon");
                    parcelDetailV2.Addresses.Should().BeEquivalentTo(
                        message.AddressPersistentLocalIds.Select(x =>
                            new ParcelDetailAddress(message.ParcelId, x)));
                    parcelDetailV2.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(message.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelWasImported()
        {
            var message = _fixture.Create<ParcelWasImported>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() }
            };

            await Sut
                .Given(new Envelope<ParcelWasImported>(new Envelope(message, metadata)))
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var parcelDetailV2 = await context.ParcelDetails.FindAsync(message.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.CaPaKey.Should().Be(message.CaPaKey);
                    parcelDetailV2.Status.Should().Be(ParcelStatus.Realized);
                    parcelDetailV2.Removed.Should().Be(false);
                    parcelDetailV2.Gml.Should().Be(geometry.ConvertToGml());
                    parcelDetailV2.GmlType.Should().Be("Polygon");
                    parcelDetailV2.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(message.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelWasRetiredV2()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();

            var parcelWasRetiredV2 = _fixture.Create<ParcelWasRetiredV2>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasImported),
                    CreateEnvelope(parcelWasRetiredV2))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetails.FindAsync(parcelWasRetiredV2.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.CaPaKey.Should().Be(parcelWasRetiredV2.CaPaKey);
                    parcelDetailV2.Status.Should().Be(ParcelStatus.Retired);
                    parcelDetailV2.LastEventHash.Should().Be(parcelWasRetiredV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelWasGeometryWasChanged()
        {
            var caPaKey = _fixture.Create<VbrCaPaKey>();
            var parcelId = _fixture.Create<ParcelId>();

            var parcelGeometryWasChanged = new ParcelGeometryWasChanged(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());

            await Sut
                .Given(
                    CreateEnvelope(_fixture.Create<ParcelWasImported>()),
                    CreateEnvelope(parcelGeometryWasChanged))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetails.FindAsync((Guid)_fixture.Create<ParcelId>());
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Gml.Should().Be(GeometryHelpers.ValidGmlPolygon);
                    parcelDetailV2.GmlType.Should().Be("Polygon");
                    parcelDetailV2.VersionTimestamp.Should().Be(parcelGeometryWasChanged.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(parcelGeometryWasChanged.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedFromRetiredToRealized()
        {
            var caPaKey = _fixture.Create<VbrCaPaKey>();
            var parcelId = _fixture.Create<ParcelId>();

            var @event = new ParcelWasCorrectedFromRetiredToRealized(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());

            await Sut
                .Given(
                    CreateEnvelope(_fixture.Create<ParcelWasImported>()),
                    CreateEnvelope(@event))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetails.FindAsync((Guid)_fixture.Create<ParcelId>());
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Gml.Should().Be(GeometryHelpers.ValidGmlPolygon);
                    parcelDetailV2.GmlType.Should().Be("Polygon");
                    parcelDetailV2.Status.Should().Be(ParcelStatus.Realized);
                    parcelDetailV2.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(@event.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttachedV2()
        {
            var message = _fixture.Create<ParcelWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() }
            };

            var addressWasAttached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasAttached.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasMigrated>(new Envelope(message, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(addressWasAttached, metadata2)))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetails.FindAsync(message.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Addresses.Should().BeEquivalentTo(
                        message.AddressPersistentLocalIds
                            .Concat(new[] { addressWasAttached.AddressPersistentLocalId })
                            .Select(x => new ParcelDetailAddress(addressWasAttached.ParcelId, x)));
                    parcelDetailV2.VersionTimestamp.Should().Be(addressWasAttached.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(addressWasAttached.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedV2()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() }
            };

            var addressWasDetached = new ParcelAddressWasDetachedV2(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(parcelWasMigrated.AddressPersistentLocalIds.First()));
            ((ISetProvenance) addressWasDetached).SetProvenance(_fixture.Create<Provenance>());
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDetached.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasDetachedV2>(new Envelope(addressWasDetached, metadata2)))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetails.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Addresses.Select(x => x.AddressPersistentLocalId).Should()
                        .NotContain(addressWasDetached.AddressPersistentLocalId);
                    parcelDetailV2.VersionTimestamp.Should().Be(addressWasDetached.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(addressWasDetached.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseOfMunicipalityMerger()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            var migratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() }
            };

            var addressWasReplaced = new ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder(_fixture)
                .WithParcelId(new ParcelId(parcelWasMigrated.ParcelId))
                .WithVbrCaPaKey(new VbrCaPaKey(parcelWasMigrated.CaPaKey))
                .WithPreviousAddress(new AddressPersistentLocalId(parcelWasMigrated.AddressPersistentLocalIds.First()))
                .WithNewAddress(new AddressPersistentLocalId(parcelWasMigrated.AddressPersistentLocalIds.Max(x => x) + 1))
                .Build();
            var replacedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasReplaced.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, migratedMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>(new Envelope(addressWasReplaced, replacedMetadata)))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetails.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Addresses.Select(x => x.AddressPersistentLocalId).Should()
                        .NotContain(addressWasReplaced.PreviousAddressPersistentLocalId);
                    parcelDetailV2.Addresses.Select(x => x.AddressPersistentLocalId).Should()
                        .Contain(addressWasReplaced.NewAddressPersistentLocalId);

                    parcelDetailV2.VersionTimestamp.Should().Be(addressWasReplaced.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(addressWasReplaced.GetHash());
                });
        }

        private Envelope<TEvent> CreateEnvelope<TEvent>(TEvent @event)
            where TEvent : IMessage, IHaveHash
        {
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());
            return new Envelope<TEvent>(new Envelope(@event, new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
            }));
        }

        protected override ParcelDetailProjections CreateProjection()
            => new ParcelDetailProjections();
    }
}
