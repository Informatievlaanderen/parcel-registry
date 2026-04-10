namespace ParcelRegistry.Projections.Wfs.ParcelWfs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

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
                throw new ProjectionItemNotFoundException<ParcelWfsProjections>(parcelId.ToString("D"));

            updateAction(item);
        }
    }
}
