namespace ParcelRegistry.Api.Oslo.Parcel.List
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.ParcelDetail;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Convertors;
    using Projections.Syndication;

    public class ParcelListOsloQuery : Query<ParcelDetail, ParcelFilter>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        protected override ISorting Sorting => new ParcelSorting();

        public ParcelListOsloQuery(LegacyContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        protected override IQueryable<ParcelDetail> Filter(FilteringHeader<ParcelFilter> filtering)
        {
            var parcels = _context
                .ParcelDetail
                .AsNoTracking()
                .OrderBy(x => x.PersistentLocalId)
                .Where(x => !x.Removed);

            if (!filtering.ShouldFilter)
            {
                return parcels;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.AddressId))
            {
                var address = _syndicationContext.AddressPersistentLocalIds.SingleOrDefault(x => x.PersistentLocalId == filtering.Filter.AddressId);
                if (address is not null)
                {
                    parcels = parcels.Where(x => x.Addresses.Any(parcelDetailAddress => parcelDetailAddress.AddressId == address.AddressId));
                }
                else
                {
                    return new List<ParcelDetail>().AsQueryable();
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(PerceelStatus), filtering.Filter.Status, true, out var status))
                {
                    var parcelStatus = ((PerceelStatus)status).MapToParcelStatus();
                    parcels = parcels.Where(m => m.Status.HasValue && m.Status.Value == parcelStatus.Status);
                }
                else
                {
                    //have to filter on EF cannot return new List<>().AsQueryable() cause non-EF provider does not support .CountAsync()
                    parcels = parcels.Where(m => m.Status == "-1");
                }
            }

            return parcels;
        }
    }

    public class ParcelSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(ParcelDetail.PersistentLocalId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(ParcelDetail.PersistentLocalId), SortOrder.Ascending);
    }

    public class ParcelFilter
    {
        public string Status { get; set; }
        public string AddressId { get; set; }
    }
}
