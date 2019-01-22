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
                .ParcelDetail
                .FindAsync(parcelId, cancellationToken: ct);

            if (parcel == null)
                throw DatabaseItemNotFound(parcelId);

            updateFunc(parcel);

            return parcel;
        }

        private static ProjectionItemNotFoundException<ParcelDetailProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<ParcelDetailProjections>(streetNameId.ToString("D"));
    }
}
