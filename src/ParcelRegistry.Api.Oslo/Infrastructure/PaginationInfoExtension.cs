namespace ParcelRegistry.Api.Oslo.Infrastructure
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;

    public static class PaginationInfoExtension
    {
        public static Uri? BuildVolgendeUri(this PaginationInfo paginationInfo, int itemsInCollection, string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage(itemsInCollection)
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }
    }
}
