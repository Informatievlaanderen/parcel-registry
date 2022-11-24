namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

    public static class ParcelSyndicationExtensions
    {
        public static async Task CreateNewParcelSyndicationItem<T>(
            this LegacyContext context,
            Guid parcelId,
            Envelope<T> message,
            Action<ParcelSyndicationItem> applyEventInfoOn,
            CancellationToken ct) where T : IMessage, IHasProvenance
        {
            var parcelSyndicationItem = await context.LatestPosition(parcelId, ct);

            if (parcelSyndicationItem == null)
                throw DatabaseItemNotFound(parcelId);

            var provenance = message.Message.Provenance;

            var newParcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            newParcelSyndicationItem.ApplyProvenance(provenance);
            newParcelSyndicationItem.SetEventData(message.Message, message.EventName);

            await context
                .ParcelSyndication
                .AddAsync(newParcelSyndicationItem, ct);
        }

        public static async Task<ParcelSyndicationItem> LatestPosition(
            this LegacyContext context,
            Guid parcelId,
            CancellationToken ct)
            => context
                   .ParcelSyndication
                   .Local
                   .Where(x => x.ParcelId == parcelId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .ParcelSyndication
                   .Where(x => x.ParcelId == parcelId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        public static void ApplyProvenance(
            this ParcelSyndicationItem item,
            ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Reason = provenance.Reason;
        }

        public static void SetEventData<T>(this ParcelSyndicationItem syndicationItem, T message, string eventName)
            => syndicationItem.EventDataAsXml = message.ToXml(eventName).ToString(SaveOptions.DisableFormatting);

        private static ProjectionItemNotFoundException<ParcelSyndicationProjections> DatabaseItemNotFound(Guid parcelId)
            => new ProjectionItemNotFoundException<ParcelSyndicationProjections>(parcelId.ToString("D"));
    }
}
