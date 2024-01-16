namespace ParcelRegistry.Projections.Integration.ParcelLatestItem
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Parcel;

    public static class ParcelLatestItemExtensions
    {
        public static async Task<ParcelLatestItem> FindAndUpdateParcel(
            this IntegrationContext context,
            Guid parcelId,
            Action<ParcelLatestItem> updateFunc,
            CancellationToken ct)
        {
            var latestItem = await context
                .ParcelLatestItems
                .FindAsync(parcelId, cancellationToken: ct);

            if (latestItem == null)
                throw DatabaseItemNotFound(parcelId);

            updateFunc(latestItem);

            return latestItem;
        }

        private static ProjectionItemNotFoundException<ParcelLatestItemProjections> DatabaseItemNotFound(Guid parcelId)
            => new ProjectionItemNotFoundException<ParcelLatestItemProjections>(parcelId.ToString("D"));

        public static string ConvertFromParcelStatus(this ParcelStatus status)
        {
            if (status == ParcelStatus.Retired)
                return PerceelStatus.Gehistoreerd.ToString();

            return PerceelStatus.Gerealiseerd.ToString();
        }
    }
}
