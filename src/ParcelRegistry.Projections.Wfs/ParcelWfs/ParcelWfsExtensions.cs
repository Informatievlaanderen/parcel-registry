namespace ParcelRegistry.Projections.Wfs.ParcelWfs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ParcelWfsExtensions
    {
        public static async Task FindAndUpdateParcelWfs(
            this WfsContext context,
            Guid parcelId,
            Action<ParcelWfsItem> updateAction,
            CancellationToken ct)
        {
            var item = await context.ParcelWfsItems.FindAsync([parcelId], ct);
            if (item == null)
                throw new InvalidOperationException($"ParcelWfsItem with ParcelId '{parcelId}' not found.");

            updateAction(item);
        }
    }
}
