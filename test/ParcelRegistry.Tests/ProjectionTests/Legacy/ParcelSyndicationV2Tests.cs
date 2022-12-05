namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Parcel.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using Fixtures;
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
        }

        [Fact]
        public async Task WhenParcelWasMigrated()
        {
            const long position = 1L;
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasMigrated) },
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem = await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem!.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    parcelSyndicationItem.Status.Should().Be(ParcelRegistry.Legacy.ParcelStatus.Parse(parcelWasMigrated.ParcelStatus));
                    parcelSyndicationItem.XCoordinate.Should().Be(parcelWasMigrated.XCoordinate);
                    parcelSyndicationItem.YCoordinate.Should().Be(parcelWasMigrated.YCoordinate);
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().BeEquivalentTo(parcelWasMigrated.AddressPersistentLocalIds);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelWasMigrated));
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                });
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
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().BeEquivalentTo(parcelWasMigrated.AddressPersistentLocalIds.Concat(new []{addressWasAttached.AddressPersistentLocalId}));
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

        protected override ParcelSyndicationProjections CreateProjection()
            => new ParcelSyndicationProjections();
    }
}
