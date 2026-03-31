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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("Feed endpoint percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor de percelen cloudevent feed voorziet.")]
    public class ParcelFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;

        public ParcelFeedProjections(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var status = MapStatus(message.Message.ParcelStatus);
                var addressPersistentLocalIds = message.Message.AddressPersistentLocalIds.ToList();

                var document = new ParcelDocument(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    status,
                    addressPersistentLocalIds,
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                await context.ParcelDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new(ParcelAttributeNames.StatusName, null, status),
                    new(ParcelAttributeNames.AdresIds, null, BuildAddressPuris(addressPersistentLocalIds)),
                ];

                await AddCloudEvent(message, document, context, attributes, ParcelEventTypes.CreateV1);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var document = new ParcelDocument(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    MapStatus(ParcelStatus.Realized),
                    new List<int>(),
                    false,
                    message.Message.Provenance.Timestamp);

                await context.ParcelDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new(ParcelAttributeNames.StatusName, null, document.Document.Status),
                    new(ParcelAttributeNames.AdresIds, null, new List<string>()),
                ];

                await AddCloudEvent(message, document, context, attributes, ParcelEventTypes.CreateV1);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = MapStatus(ParcelStatus.Retired);
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
                document.Document.Status = MapStatus(ParcelStatus.Realized);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.StatusName, oldStatus, document.Document.Status),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Add(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);

                document.Document.AddressPersistentLocalIds.Remove(message.Message.PreviousAddressPersistentLocalId);
                if (!document.Document.AddressPersistentLocalIds.Contains(message.Message.NewAddressPersistentLocalId))
                {
                    document.Document.AddressPersistentLocalIds.Add(message.Message.NewAddressPersistentLocalId);
                }

                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);

                document.Document.AddressPersistentLocalIds.Remove(message.Message.PreviousAddressPersistentLocalId);
                document.Document.AddressPersistentLocalIds.Add(message.Message.NewAddressPersistentLocalId); //this can cause doubles, but we'll build the uri's unique

                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.CaPaKey, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);

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

                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new(ParcelAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                ], ParcelEventTypes.UpdateV1);
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
            string eventType) where T : IMessage, IHasProvenance, IHasParcelId
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var page = await context.CalculatePage();

            var feedItem = new ParcelFeedItem(message.Position, page, message.Message.ParcelId, document.CaPaKey)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.ParcelFeed.AddAsync(feedItem);

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

        private static PerceelStatus MapStatus(ParcelStatus parcelStatus)
        {
            if(parcelStatus == ParcelStatus.Realized)
                return PerceelStatus.Gerealiseerd;
            if(parcelStatus == ParcelStatus.Retired)
                return PerceelStatus.Gehistoreerd;

            throw new InvalidOperationException($"Unknown parcel status: {parcelStatus}");
        }

        private static List<string> BuildAddressPuris(IEnumerable<int> addressPersistentLocalIds)
        {
            return addressPersistentLocalIds
                .Select(id => OsloNamespaces.Adres.ToPuri(id.ToString()))
                .Distinct()
                .ToList();
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
