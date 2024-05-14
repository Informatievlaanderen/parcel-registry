namespace ParcelRegistry.Projections.Legacy.ParcelDetailWithCountV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class ParcelDetailV2Extensions
    {
        public static async Task<ParcelDetailV2> FindAndUpdateParcelDetail(
            this LegacyContext context,
            Guid parcelId,
            Action<ParcelDetailV2> updateFunc,
            CancellationToken ct)
        {
            var parcel = await context
                .ParcelDetailWithCountV2
                .FindAsync(parcelId, cancellationToken: ct);

            if (parcel == null)
                throw DatabaseItemNotFound(parcelId);

            updateFunc(parcel);
            return parcel;
        }

        private static ProjectionItemNotFoundException<ParcelDetailV2Projections> DatabaseItemNotFound(Guid parcelId)
            => new ProjectionItemNotFoundException<ParcelDetailV2Projections>(parcelId.ToString("D"));
    }
}
