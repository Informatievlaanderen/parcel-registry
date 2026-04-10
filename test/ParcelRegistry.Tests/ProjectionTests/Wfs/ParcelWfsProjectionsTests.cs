namespace ParcelRegistry.Tests.ProjectionTests.Wfs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Parcel;
    using Parcel.Events;
    using Projections.Wfs;
    using Projections.Wfs.ParcelWfs;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public sealed class ParcelWfsProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly WfsContext _wfsContext;

        private ConnectedProjectionTest<WfsContext, ParcelWfsProjections> Sut { get; }

        public ParcelWfsProjectionsTests()
        {
            _wfsContext = CreateContext();

            Sut = new ConnectedProjectionTest<WfsContext, ParcelWfsProjections>(
                () => _wfsContext,
                () => new ParcelWfsProjections());

            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithParcelStatus());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            _fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public async Task WhenParcelWasMigrated_ThenParcelWfsItemAndAddressesAreAdded()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(parcelWasMigrated, position))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelWasMigrated.ParcelId);
                    item.Should().NotBeNull();

                    var expectedCaPaKey = CaPaKey.CreateFrom(parcelWasMigrated.CaPaKey);
                    item!.CaPaKey.Should().Be(expectedCaPaKey.CaPaKeyCrabNotation2);
                    item.VbrCaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    item.StatusAsString.Should().Be(ParcelStatus.Parse(parcelWasMigrated.ParcelStatus).Status);
                    item.Removed.Should().Be(parcelWasMigrated.IsRemoved);
                    item.VersionTimestamp.Should().Be(parcelWasMigrated.Provenance.Timestamp);

                    var addresses = await context.ParcelWfsAddresses
                        .Where(x => x.ParcelId == parcelWasMigrated.ParcelId)
                        .ToListAsync();
                    addresses.Should().HaveCount(parcelWasMigrated.AddressPersistentLocalIds.Count);

                    foreach (var addressPersistentLocalId in parcelWasMigrated.AddressPersistentLocalIds)
                    {
                        addresses.Should().Contain(x => x.AddressPersistentLocalId == addressPersistentLocalId);
                    }
                });
        }

        [Fact]
        public async Task WhenParcelWasImported_ThenParcelWfsItemIsAdded()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(parcelWasImported, position))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelWasImported.ParcelId);
                    item.Should().NotBeNull();

                    var expectedCaPaKey = CaPaKey.CreateFrom(parcelWasImported.CaPaKey);
                    item!.CaPaKey.Should().Be(expectedCaPaKey.CaPaKeyCrabNotation2);
                    item.VbrCaPaKey.Should().Be(parcelWasImported.CaPaKey);
                    item.Status.Should().Be(ParcelStatus.Realized);
                    item.Removed.Should().BeFalse();
                    item.VersionTimestamp.Should().Be(parcelWasImported.Provenance.Timestamp);

                    var addresses = await context.ParcelWfsAddresses
                        .Where(x => x.ParcelId == parcelWasImported.ParcelId)
                        .ToListAsync();
                    addresses.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenParcelWasRetiredV2_ThenStatusIsUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var parcelWasRetired = _fixture.Create<ParcelWasRetiredV2>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelWasRetired, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelWasRetired.ParcelId);
                    item.Should().NotBeNull();
                    item!.Status.Should().Be(ParcelStatus.Retired);
                    item.VersionTimestamp.Should().Be(parcelWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelGeometryWasChanged_ThenVersionTimestampIsUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var parcelGeometryWasChanged = _fixture.Create<ParcelGeometryWasChanged>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelGeometryWasChanged, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelGeometryWasChanged.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(parcelGeometryWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedFromRetiredToRealized_ThenStatusIsUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Retired);
            var parcelWasCorrected = _fixture.Create<ParcelWasCorrectedFromRetiredToRealized>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelWasCorrected, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelWasCorrected.ParcelId);
                    item.Should().NotBeNull();
                    item!.Status.Should().Be(ParcelStatus.Realized);
                    item.VersionTimestamp.Should().Be(parcelWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttachedV2_ThenAddressIsAdded()
        {
            var parcelWasMigrated = CreateParcelWasMigratedWithoutAddresses();
            var parcelAddressWasAttached = _fixture.Create<ParcelAddressWasAttachedV2>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasAttached, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelAddressWasAttached.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(parcelAddressWasAttached.Provenance.Timestamp);

                    var address = await context.ParcelWfsAddresses.FindAsync(
                        parcelAddressWasAttached.ParcelId, parcelAddressWasAttached.AddressPersistentLocalId);
                    address.Should().NotBeNull();
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedV2_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var addressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            _fixture.Register(() => new AddressPersistentLocalId(addressPersistentLocalId));

            var parcelAddressWasDetached = _fixture.Create<ParcelAddressWasDetachedV2>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasDetached, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(parcelAddressWasDetached.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(parcelAddressWasDetached.Provenance.Timestamp);

                    var address = await context.ParcelWfsAddresses.FindAsync(
                        parcelAddressWasDetached.ParcelId, parcelAddressWasDetached.AddressPersistentLocalId);
                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRemoved_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var addressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            _fixture.Register(() => new AddressPersistentLocalId(addressPersistentLocalId));

            var @event = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRemoved>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(@event.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);

                    var address = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, @event.AddressPersistentLocalId);
                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRejected_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var addressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            _fixture.Register(() => new AddressPersistentLocalId(addressPersistentLocalId));

            var @event = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRejected>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(@event.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);

                    var address = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, @event.AddressPersistentLocalId);
                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRetired_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var addressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            _fixture.Register(() => new AddressPersistentLocalId(addressPersistentLocalId));

            var @event = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRetired>();

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(@event.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);

                    var address = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, @event.AddressPersistentLocalId);
                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseOfMunicipalityMerger_ThenAddressIsReplaced()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var previousAddressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            var newAddressPersistentLocalId = _fixture.Create<int>();

            _fixture.Register(() => new AddressPersistentLocalId(previousAddressPersistentLocalId));
            var @event = new ParcelAddressWasReplacedBecauseOfMunicipalityMerger(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(newAddressPersistentLocalId),
                new AddressPersistentLocalId(previousAddressPersistentLocalId));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(@event.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);

                    var oldAddress = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, previousAddressPersistentLocalId);
                    oldAddress.Should().BeNull();

                    var newAddress = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, newAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseAddressWasReaddressed_ThenAddressIsReplaced()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var previousAddressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            var newAddressPersistentLocalId = _fixture.Create<int>();

            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(newAddressPersistentLocalId),
                new AddressPersistentLocalId(previousAddressPersistentLocalId));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(@event.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);

                    var oldAddress = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, previousAddressPersistentLocalId);
                    oldAddress.Should().BeNull();

                    var newAddress = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, newAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseAddressWasReaddressed_WithCount2_ThenCountIsAggregatedTo2AndPreviousAddressesAreRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigratedWithoutAddresses();
            var addressA = _fixture.Create<int>();
            var addressB = _fixture.Create<int>();
            var addressC = _fixture.Create<int>();

            // First: attach addressA (via simple attach)
            var attachA = new ParcelAddressWasAttachedV2(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(addressA));
            ((ISetProvenance)attachA).SetProvenance(_fixture.Create<Provenance>());

            // Second: attach addressB (via simple attach)
            var attachB = new ParcelAddressWasAttachedV2(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(addressB));
            ((ISetProvenance)attachB).SetProvenance(_fixture.Create<Provenance>());

            // Readdress: A -> C (creates C with count=1, removes A)
            var readdress1 = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(addressC),
                new AddressPersistentLocalId(addressA));
            ((ISetProvenance)readdress1).SetProvenance(_fixture.Create<Provenance>());

            // Readdress: B -> C (C already exists, so count should become 2; B count=1 so removed)
            var readdress2 = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new AddressPersistentLocalId(addressC),
                new AddressPersistentLocalId(addressB));
            ((ISetProvenance)readdress2).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(attachA, 2L),
                    CreateEnvelope(attachB, 3L),
                    CreateEnvelope(readdress1, 4L),
                    CreateEnvelope(readdress2, 5L))
                .Then(async context =>
                {
                    var addrA = await context.ParcelWfsAddresses.FindAsync(
                        readdress2.ParcelId, addressA);
                    addrA.Should().BeNull();

                    var addrB = await context.ParcelWfsAddresses.FindAsync(
                        readdress2.ParcelId, addressB);
                    addrB.Should().BeNull();

                    var addrC = await context.ParcelWfsAddresses.FindAsync(
                        readdress2.ParcelId, addressC);
                    addrC.Should().NotBeNull();
                    addrC!.Count.Should().Be(2);
                });
        }

        [Fact]
        public async Task WhenParcelAddressesWereReaddressed_ThenAddressesAreUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated(ParcelStatus.Realized);
            var detachedAddressPersistentLocalId = parcelWasMigrated.AddressPersistentLocalIds.First();
            var attachedAddressPersistentLocalId = _fixture.Create<int>();

            var @event = new ParcelAddressesWereReaddressed(
                new ParcelId(parcelWasMigrated.ParcelId),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                new[] { new AddressPersistentLocalId(attachedAddressPersistentLocalId) },
                new[] { new AddressPersistentLocalId(detachedAddressPersistentLocalId) },
                new List<AddressRegistryReaddress>());
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(@event, 2L))
                .Then(async context =>
                {
                    var item = await context.ParcelWfsItems.FindAsync(@event.ParcelId);
                    item.Should().NotBeNull();
                    item!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);

                    var detached = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, detachedAddressPersistentLocalId);
                    detached.Should().BeNull();

                    var attached = await context.ParcelWfsAddresses.FindAsync(
                        @event.ParcelId, attachedAddressPersistentLocalId);
                    attached.Should().NotBeNull();
                });
        }

        #region Helpers

        private ParcelWasMigrated CreateParcelWasMigrated(ParcelStatus status)
        {
            _fixture.Register(() => status);
            return _fixture.Create<ParcelWasMigrated>();
        }

        private ParcelWasMigrated CreateParcelWasMigratedWithoutAddresses()
        {
            _fixture.Register(() => ParcelStatus.Realized);
            _fixture.Register<IEnumerable<AddressPersistentLocalId>>(() => new List<AddressPersistentLocalId>());
            return _fixture.Create<ParcelWasMigrated>();
        }

        private Envelope<T> CreateEnvelope<T>(T @event, long position) where T : IMessage
        {
            var metadata = new Dictionary<string, object>
            {
                { "Position", position },
                { "EventName", @event.GetType().Name },
                { "CommandId", Guid.NewGuid().ToString() },
                { Envelope.CreatedUtcMetadataKey, DateTime.UtcNow }
            };
            return new Envelope<T>(new Envelope(@event, metadata));
        }

        private static WfsContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WfsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WfsContext(options);
        }

        #endregion
    }
}
