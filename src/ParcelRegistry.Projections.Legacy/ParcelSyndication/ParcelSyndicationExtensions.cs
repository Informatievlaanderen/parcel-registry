namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ParcelSyndicationExtensions
    {
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
    }
}
