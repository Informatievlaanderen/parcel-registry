namespace ParcelRegistry.Api.Legacy.Parcel.Query
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.ParcelDetail;
    using System.Collections.Generic;
    using System.Linq;

    public class ParcelListQuery : Query<ParcelDetail, ParcelFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new ParcelSorting();

        public ParcelListQuery(LegacyContext context) => _context = context;

        protected override IQueryable<ParcelDetail> Filter(FilteringHeader<ParcelFilter> filtering)
        {
            var parcels = _context
                .ParcelDetail
                .AsNoTracking()
                .Where(x => x.Complete);

            if (!filtering.ShouldFilter)
                return parcels;

            return parcels;
        }

        internal class ParcelSorting : ISorting
        {
            public IEnumerable<string> SortableFields { get; } = new[]
            {
                nameof(ParcelDetail.PersistentLocalId),
            };

            public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(ParcelDetail.PersistentLocalId), SortOrder.Ascending);
        }
    }

    public class ParcelFilter { }
}
