namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Microsoft.EntityFrameworkCore;

    public static class ParcelFeedExtensions
    {
        public static async Task<int> CalculatePage(this FeedContext context, int maxPageSize = ChangeFeedService.DefaultMaxPageSize)
        {
            if (!await context.ParcelFeed.AnyAsync() && context.ParcelFeed.Local.Count == 0)
            {
                return 1;
            }

            var maxPage = await context.ParcelFeed.MaxAsync(x => x.Page);
            var dbCount = await context.ParcelFeed.CountAsync(x => x.Page == maxPage);

            var localCount = context.ParcelFeed.Local
                .Count(x => x.Page == maxPage && context.Entry(x).State == EntityState.Added);

            var totalCount = dbCount + localCount;

            return totalCount >= maxPageSize ? maxPage + 1 : maxPage;
        }
    }
}
