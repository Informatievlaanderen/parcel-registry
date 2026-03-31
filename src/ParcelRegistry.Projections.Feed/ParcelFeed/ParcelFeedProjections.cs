namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using Parcel.Events;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    [ConnectedProjectionName("Feed endpoint percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor de percelen cloudevent feed voorziet.")]
    public class ParcelFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;

        private static string MapStatus(string parcelStatus)
        {
            return parcelStatus switch
            {
                "Realized" => "Gerealiseerd",
                "Retired" => "Gehistoreerd",
                _ => parcelStatus
            };
        }

        public ParcelFeedProjections(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var status = MapStatus(message.Message.ParcelStatus);

                var document = new ParcelDocument(
                    message.Message.CaPaKey,
                    status,
                    message.Message.AddressPersistentLocalIds.ToList(),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                await context.ParcelDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new(ParcelAttributeNames.StatusName, null, status),
                ];

                await AddCloudEvent(message, document, context, attributes, ParcelEventTypes.CreateV1);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var document = new ParcelDocument(
                    message.Message.CaPaKey,
                    MapStatus("Realized"),
                    new List<int>(),
                    false,
                    message.Message.Provenance.Timestamp);

                await context.ParcelDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new(ParcelAttributeNames.StatusName, null, document.Document.Status),
                ];

                await AddCloudEvent(message, document, context, attributes, ParcelEventTypes.CreateV1);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = MapStatus("Retired");
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.StatusName, oldStatus, document.Document.Status),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);

                var oldStatus = document.Document.Status;
                document.Document.Status = MapStatus("Realized");
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.StatusName, oldStatus, document.Document.Status),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                document.Document.AddressPersistentLocalIds.Add(message.Message.AddressPersistentLocalId);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);

                document.Document.AddressPersistentLocalIds.Remove(message.Message.PreviousAddressPersistentLocalId);
                if (!document.Document.AddressPersistentLocalIds.Contains(message.Message.NewAddressPersistentLocalId))
                {
                    document.Document.AddressPersistentLocalIds.Add(message.Message.NewAddressPersistentLocalId);
                }

                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);

                document.Document.AddressPersistentLocalIds.Remove(message.Message.PreviousAddressPersistentLocalId);
                if (!document.Document.AddressPersistentLocalIds.Contains(message.Message.NewAddressPersistentLocalId))
                {
                    document.Document.AddressPersistentLocalIds.Add(message.Message.NewAddressPersistentLocalId);
                }

                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);

                foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                {
                    document.Document.AddressPersistentLocalIds.Remove(addressPersistentLocalId);
                }

                foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                {
                    if (!document.Document.AddressPersistentLocalIds.Contains(addressPersistentLocalId))
                    {
                        document.Document.AddressPersistentLocalIds.Add(addressPersistentLocalId);
                    }
                }

                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], ParcelEventTypes.UpdateV1);
            });
        }

        private static async Task<ParcelDocument> FindDocument(FeedContext context, string caPaKey, CancellationToken ct)
        {
            var document = await context.ParcelDocuments.FindAsync([caPaKey], ct);
            if (document is null)
                throw new InvalidOperationException($"ParcelDocument with CaPaKey '{caPaKey}' not found.");
            return document;
        }

        private async Task AddCloudEvent<T>(
            Envelope<T> message,
            ParcelDocument document,
            FeedContext context,
            List<BaseRegistriesCloudEventAttribute> attributes,
            string eventType) where T : IMessage, IHasProvenance
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var page = await context.CalculatePage();

            var feedItem = new ParcelFeedItem(message.Position, page)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.ParcelFeed.AddAsync(feedItem);
            await context.ParcelFeedItemParcels.AddAsync(new ParcelFeedItemParcel(feedItem.Id, document.CaPaKey));

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                feedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.CaPaKey,
                document.LastChangedOnAsDateTimeOffset,
                new List<string>(),
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            feedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);

            await CheckToUpdateCache(page, context);
        }

        private async Task CheckToUpdateCache(int page, FeedContext context)
        {
            await _changeFeedService.CheckToUpdateCacheAsync(
                page,
                context,
                async p =>
                {
                    var localCount = context.ParcelFeed.Local
                        .Count(x => x.Page == page && context.Entry(x).State == EntityState.Added);
                    return await context.ParcelFeed.CountAsync(x => x.Page == p) + localCount - 1;
                });
        }
    }
}
