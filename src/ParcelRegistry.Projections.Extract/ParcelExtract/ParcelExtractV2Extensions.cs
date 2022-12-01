namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ParcelExtractV2Extensions
    {
        public static async Task<ParcelExtractItemV2> FindAndUpdateParcelExtract(
            this ExtractContext context,
            Guid parcelId,
            Action<ParcelExtractItemV2> updateFunc,
            CancellationToken ct)
        {
            var parcel = await context
                .ParcelExtractV2
                .FindAsync(parcelId, cancellationToken: ct);

            if (parcel == null)
            {
                throw DatabaseItemNotFound(parcelId);
            }

            updateFunc(parcel);

            return parcel;
        }

        private static ProjectionItemNotFoundException<ParcelExtractV2Projections> DatabaseItemNotFound(Guid parcelId)
            => new ProjectionItemNotFoundException<ParcelExtractV2Projections>(parcelId.ToString("D"));
    }
}
