namespace ParcelRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using CloudNative.CloudEvents;
    using FluentAssertions;
    using Fixtures;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Newtonsoft.Json;
    using Parcel;
    using Parcel.Events;
    using Projections.Feed;
    using Projections.Feed.Contract;
    using Projections.Feed.ParcelFeed;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public sealed class ParcelFeedProjectionsTests
    {
        private readonly Fixture _fixture;
        private readonly FeedContext _feedContext;

        private ConnectedProjectionTest<FeedContext, ParcelFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }

        public ParcelFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            _feedContext = CreateContext();

            Sut = new ConnectedProjectionTest<FeedContext, ParcelFeedProjections>(
                () => _feedContext,
                () => new ParcelFeedProjections(ChangeFeedServiceMock.Object));

            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithParcelStatus());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            _fixture.Customize(new WithExtendedWkbGeometryPolygon());

            SetupChangeFeedServiceMock();
        }

        [Fact]
        public async Task WhenParcelWasMigrated_ThenFeedItemAndDocumentAreAdded()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(parcelWasMigrated, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelWasMigrated.CaPaKey);
                    document.Should().NotBeNull();
                    document!.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    document.IsRemoved.Should().Be(parcelWasMigrated.IsRemoved);
                    document.RecordCreatedAt.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    document.Document.VersionId.Should().Be(parcelWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset());
                    document.Document.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    document.Document.Status.Should().Be(MapStatus(parcelWasMigrated.ParcelStatus));
                    document.Document.AddressPersistentLocalIds.Should().BeEquivalentTo(parcelWasMigrated.AddressPersistentLocalIds);

                    var feedItem = await FindFeedItemByCaPaKey(context, parcelWasMigrated.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelWasMigrated);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            parcelWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            ParcelEventTypes.CreateV1,
                            parcelWasMigrated.CaPaKey,
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<List<string>>(),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == ParcelAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == MapStatus(parcelWasMigrated.ParcelStatus))),
                            It.IsAny<string>(),
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenParcelWasImported_ThenFeedItemAndDocumentAreAdded()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(parcelWasImported, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelWasImported.CaPaKey);
                    document.Should().NotBeNull();
                    document!.CaPaKey.Should().Be(parcelWasImported.CaPaKey);
                    document.IsRemoved.Should().BeFalse();
                    document.RecordCreatedAt.Should().Be(parcelWasImported.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(parcelWasImported.Provenance.Timestamp);
                    document.Document.Status.Should().Be("Gerealiseerd");
                    document.Document.AddressPersistentLocalIds.Should().BeEmpty();

                    var feedItem = await FindFeedItemByCaPaKey(context, parcelWasImported.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelWasImported);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            parcelWasImported.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            ParcelEventTypes.CreateV1,
                            parcelWasImported.CaPaKey,
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<List<string>>(),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == ParcelAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == "Gerealiseerd")),
                            It.IsAny<string>(),
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenParcelWasRetiredV2_ThenDocumentStatusIsUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelWasRetired = _fixture.Create<ParcelWasRetiredV2>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelWasRetired, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelWasRetired.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be("Gehistoreerd");
                    document.LastChangedOn.Should().Be(parcelWasRetired.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelWasRetired.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelWasRetired);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            parcelWasRetired.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            ParcelEventTypes.UpdateV1,
                            parcelWasRetired.CaPaKey,
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<List<string>>(),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == ParcelAttributeNames.StatusName
                                               && a.OldValue!.ToString() == "Gerealiseerd"
                                               && a.NewValue!.ToString() == "Gehistoreerd")),
                            It.IsAny<string>(),
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenParcelGeometryWasChanged_ThenVersionIsUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelGeometryWasChanged = _fixture.Create<ParcelGeometryWasChanged>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelGeometryWasChanged, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelGeometryWasChanged.CaPaKey);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(parcelGeometryWasChanged.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelGeometryWasChanged.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelGeometryWasChanged);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            parcelGeometryWasChanged.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            ParcelEventTypes.UpdateV1,
                            parcelGeometryWasChanged.CaPaKey,
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<List<string>>(),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs => attrs.Count == 0),
                            It.IsAny<string>(),
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedFromRetiredToRealized_ThenStatusIsUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Retired");
            var parcelWasCorrected = _fixture.Create<ParcelWasCorrectedFromRetiredToRealized>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelWasCorrected, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelWasCorrected.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be("Gerealiseerd");
                    document.LastChangedOn.Should().Be(parcelWasCorrected.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelWasCorrected.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelWasCorrected);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            parcelWasCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            ParcelEventTypes.UpdateV1,
                            parcelWasCorrected.CaPaKey,
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<List<string>>(),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == ParcelAttributeNames.StatusName
                                               && a.OldValue!.ToString() == "Gehistoreerd"
                                               && a.NewValue!.ToString() == "Gerealiseerd")),
                            It.IsAny<string>(),
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttachedV2_ThenAddressIsAdded()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasAttached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasAttached, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasAttached.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(parcelAddressWasAttached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasAttached.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasAttached.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasAttached);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedV2_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasDetached = _fixture.Create<ParcelAddressWasDetachedV2>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasDetached, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasDetached.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(parcelAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasDetached.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasDetached.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasDetached);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRemoved_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasDetached = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRemoved>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasDetached, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasDetached.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(parcelAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasDetached.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasDetached.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasDetached);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRejected_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasDetached = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRejected>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasDetached, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasDetached.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(parcelAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasDetached.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasDetached.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasDetached);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRetired_ThenAddressIsRemoved()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasDetached = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRetired>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasDetached, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasDetached.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(parcelAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasDetached.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasDetached.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasDetached);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseOfMunicipalityMerger_ThenAddressIsReplaced()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasReplaced = _fixture.Create<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasReplaced, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasReplaced.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(parcelAddressWasReplaced.NewAddressPersistentLocalId);
                    document.Document.AddressPersistentLocalIds.Should().NotContain(parcelAddressWasReplaced.PreviousAddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasReplaced.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasReplaced.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasReplaced);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseAddressWasReaddressed_ThenAddressIsReplaced()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressWasReplaced = _fixture.Create<ParcelAddressWasReplacedBecauseAddressWasReaddressed>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressWasReplaced, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressWasReplaced.CaPaKey);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(parcelAddressWasReplaced.NewAddressPersistentLocalId);
                    document.Document.AddressPersistentLocalIds.Should().NotContain(parcelAddressWasReplaced.PreviousAddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(parcelAddressWasReplaced.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressWasReplaced.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressWasReplaced);
                });
        }

        [Fact]
        public async Task WhenParcelAddressesWereReaddressed_ThenAddressesAreUpdated()
        {
            var parcelWasMigrated = CreateParcelWasMigrated("Realized");
            var parcelAddressesWereReaddressed = _fixture.Create<ParcelAddressesWereReaddressed>();
            var position = 2L;

            await Sut
                .Given(
                    CreateEnvelope(parcelWasMigrated, 1L),
                    CreateEnvelope(parcelAddressesWereReaddressed, position))
                .Then(async context =>
                {
                    var document = await context.ParcelDocuments.FindAsync(parcelAddressesWereReaddressed.CaPaKey);
                    document.Should().NotBeNull();

                    foreach (var attached in parcelAddressesWereReaddressed.AttachedAddressPersistentLocalIds)
                    {
                        document!.Document.AddressPersistentLocalIds.Should().Contain(attached);
                    }

                    foreach (var detached in parcelAddressesWereReaddressed.DetachedAddressPersistentLocalIds)
                    {
                        document!.Document.AddressPersistentLocalIds.Should().NotContain(detached);
                    }

                    document!.LastChangedOn.Should().Be(parcelAddressesWereReaddressed.Provenance.Timestamp);

                    var feedItem = await FindLastFeedItemByCaPaKey(context, parcelAddressesWereReaddressed.CaPaKey);
                    AssertFeedItem(feedItem, position, parcelAddressesWereReaddressed);
                });
        }

        #region Helpers

        private static string MapStatus(string parcelStatus)
        {
            return parcelStatus switch
            {
                "Realized" => "Gerealiseerd",
                "Retired" => "Gehistoreerd",
                _ => parcelStatus
            };
        }

        private ParcelWasMigrated CreateParcelWasMigrated(string status)
        {
            _fixture.Register(() => ParcelStatus.Parse(status));
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();
            return parcelWasMigrated;
        }

        private static void AssertFeedItem(
            ParcelFeedItem? feedItem,
            long position,
            IParcelEvent @event)
        {
            feedItem.Should().NotBeNull();
            feedItem!.CloudEventAsString.Should().NotBeNullOrEmpty();
            feedItem.Page.Should().Be(1);
            feedItem.Position.Should().Be(position);
            feedItem.Application.Should().Be(@event.Provenance.Application);
            feedItem.Modification.Should().Be(@event.Provenance.Modification);
            feedItem.Operator.Should().Be(@event.Provenance.Operator);
            feedItem.Organisation.Should().Be(@event.Provenance.Organisation);
            feedItem.Reason.Should().Be(@event.Provenance.Reason);
        }

        private static async Task<ParcelFeedItem?> FindFeedItemByCaPaKey(FeedContext context, string caPaKey)
        {
            var feedItemParcel = await context.ParcelFeedItemParcels
                .Where(x => x.CaPaKey == caPaKey)
                .SingleOrDefaultAsync();

            if (feedItemParcel is null)
                return null;

            return await context.ParcelFeed.SingleOrDefaultAsync(x => x.Id == feedItemParcel.FeedItemId);
        }

        private static async Task<ParcelFeedItem> FindLastFeedItemByCaPaKey(FeedContext context, string caPaKey)
        {
            var feedItemIds = await context.ParcelFeedItemParcels
                .Where(x => x.CaPaKey == caPaKey)
                .Select(x => x.FeedItemId)
                .ToListAsync();

            return await context.ParcelFeed
                .Where(x => feedItemIds.Contains(x.Id))
                .OrderBy(x => x.Id)
                .LastAsync();
        }

        private Envelope<T> CreateEnvelope<T>(T @event, long position) where T : IMessage
        {
            var metadata = new Dictionary<string, object>
            {
                { "Position", position },
                { "EventName", @event.GetType().Name },
                { "CommandId", Guid.NewGuid().ToString() }
            };
            return new Envelope<T>(new Envelope(@event, metadata));
        }

        private void SetupChangeFeedServiceMock()
        {
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEventWithData(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new CloudEvent());

            ChangeFeedServiceMock.Setup(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>())).Returns("serialized cloud event");

            ChangeFeedServiceMock.Setup(x => x.CheckToUpdateCacheAsync(
                It.IsAny<int>(),
                It.IsAny<FeedContext>(),
                It.IsAny<Func<int, Task<int>>>()));
        }

        private FeedContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FeedContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FeedContext(options, new JsonSerializerSettings());
        }

        #endregion
    }
}
