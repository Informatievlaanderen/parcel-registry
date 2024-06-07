namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class ParcelDetailExtensions
    {
        public static async Task<ParcelDetail> FindAndUpdateParcelDetail(
            this LegacyContext context,
            Guid parcelId,
            Action<ParcelDetail> updateFunc,
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

        private static ProjectionItemNotFoundException<ParcelDetailProjections> DatabaseItemNotFound(Guid parcelId)
            => new ProjectionItemNotFoundException<ParcelDetailProjections>(parcelId.ToString("D"));
    }
}
