namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ParcelExtractExtensions
    {
        public static async Task<ParcelExtractItem> FindAndUpdateParcelExtract(
            this ExtractContext context,
            Guid parcelId,
            Action<ParcelExtractItem> updateFunc,
            CancellationToken ct)
        {
            var parcel = await context
                .ParcelExtract
                .FindAsync(parcelId, cancellationToken: ct);

            if (parcel == null)
                throw DatabaseItemNotFound(parcelId);

            updateFunc(parcel);

            return parcel;
        }

        private static ProjectionItemNotFoundException<ParcelExtractProjections> DatabaseItemNotFound(Guid streetNameId)
            => new ProjectionItemNotFoundException<ParcelExtractProjections>(streetNameId.ToString("D"));
    }
}
