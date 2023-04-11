namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Parcel.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using EventExtensions;
    using FluentAssertions;
    using Fixtures;
    using Parcel;
    using Projections.Legacy.ParcelDetailV2;
    using Xunit;

    public class ParcelDetailItemV2Tests : ParcelLegacyProjectionTest<ParcelDetailV2Projections>
    {
        private readonly Fixture? _fixture;

        public ParcelDetailItemV2Tests()
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
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var parcelDetailV2 = await ct.ParcelDetailV2.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    parcelDetailV2.Status.Should().Be(ParcelStatus.Parse(parcelWasMigrated.ParcelStatus));
                    parcelDetailV2.Removed.Should().Be(parcelWasMigrated.IsRemoved);
                    parcelDetailV2.Addresses.Should().BeEquivalentTo(
                        parcelWasMigrated.AddressPersistentLocalIds.Select(x =>
                            new ParcelDetailAddressV2(parcelWasMigrated.ParcelId, x)));
                    parcelDetailV2.VersionTimestamp.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(parcelWasMigrated.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttachedV2()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() }
            };

            var addressWasAttached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasAttached.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(addressWasAttached, metadata2)))
                .Then(async ct =>
                {
                    var parcelDetailV2 = await ct.ParcelDetailV2.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Addresses.Should().BeEquivalentTo(
                        parcelWasMigrated.AddressPersistentLocalIds
                            .Concat(new[] { addressWasAttached.AddressPersistentLocalId })
                            .Select(x => new ParcelDetailAddressV2(addressWasAttached.ParcelId, x)));
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
            ((ISetProvenance)addressWasDetached).SetProvenance(_fixture.Create<Provenance>());
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasDetached.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasDetachedV2>(new Envelope(addressWasDetached, metadata2)))
                .Then(async ct =>
                {
                    var parcelDetailV2 = await ct.ParcelDetailV2.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Addresses.Select(x => x.AddressPersistentLocalId).Should().NotContain(addressWasDetached.AddressPersistentLocalId);
                    parcelDetailV2.VersionTimestamp.Should().Be(addressWasDetached.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(addressWasDetached.GetHash());
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseAddressWasReaddressed()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(2);

            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>()
                .WithClearedAddresses()
                .WithAddress(previousAddressPersistentLocalId);
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() }
            };

            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                addressPersistentLocalId,
                previousAddressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());
            var metadata2 = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(new Envelope(@event, metadata2)))
                .Then(async ct =>
                {
                    var parcelDetailV2 = await ct.ParcelDetailV2.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.Addresses
                        .Select(x => x.AddressPersistentLocalId).Should().NotContain(@event.PreviousAddressPersistentLocalId);
                    parcelDetailV2.Addresses
                        .Select(x => x.AddressPersistentLocalId).Should().Contain(@event.NewAddressPersistentLocalId);
                    parcelDetailV2.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(@event.GetHash());
                });
        }

        protected override ParcelDetailV2Projections CreateProjection()
            => new ParcelDetailV2Projections();
    }
}
