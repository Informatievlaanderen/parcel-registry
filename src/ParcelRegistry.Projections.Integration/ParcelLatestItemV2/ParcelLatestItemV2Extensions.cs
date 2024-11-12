namespace ParcelRegistry.Projections.Integration.ParcelLatestItemV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Parcel;

    public static class ParcelLatestItemV2Extensions
    {
        public static async Task<ParcelLatestItemV2> FindAndUpdateParcel(
            this IntegrationContext context,
            Guid parcelId,
            Action<ParcelLatestItemV2> updateFunc,
            CancellationToken ct)
        {
            var latestItem = await context
                .ParcelLatestItemsV2
                .FindAsync(parcelId, cancellationToken: ct);

            if (latestItem == null)
                throw DatabaseItemNotFound(parcelId);

            updateFunc(latestItem);

            return latestItem;
        }

        private static ProjectionItemNotFoundException<ParcelLatestItemV2Projections> DatabaseItemNotFound(Guid parcelId)
            => new ProjectionItemNotFoundException<ParcelLatestItemV2Projections>(parcelId.ToString("D"));
    }
}
