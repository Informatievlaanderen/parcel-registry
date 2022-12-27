namespace ParcelRegistry.Api.Oslo.Parcel.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Convertors;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.ParcelDetailV2;

    public class ParcelListOsloV2Query : Query<ParcelDetailV2, ParcelFilterV2>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new ParcelSortingV2();

        public ParcelListOsloV2Query(LegacyContext context) => _context = context;

        protected override IQueryable<ParcelDetailV2> Filter(FilteringHeader<ParcelFilterV2> filtering)
        {
            var parcels = _context
                .ParcelDetailV2
                .AsNoTracking()
                .OrderBy(x => x.CaPaKey)
                .Where(x => !x.Removed);

            if (!filtering.ShouldFilter)
            {
                return parcels;
            }

            if (!string.IsNullOrEmpty(filtering.Filter.AddressId))
            {
                if (int.TryParse(filtering.Filter.AddressId, out var addressId))
                {
                    parcels = parcels.Where(x => x.Addresses.Any(parcelDetailAddress => parcelDetailAddress.AddressPersistentLocalId == addressId));
                }
                else
                {
                    return new List<ParcelDetailV2>().AsQueryable();
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(PerceelStatus), filtering.Filter.Status, true, out var status))
                {
                    var parcelStatus = ((PerceelStatus)status).MapToParcelStatus();
                    parcels = parcels.Where(m => m.Status == parcelStatus.Status);
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

    public class ParcelSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(ParcelDetailV2.CaPaKey),
        };

        public SortingHeader DefaultSortingHeader { get; } =
            new SortingHeader(nameof(ParcelDetailV2.CaPaKey), SortOrder.Ascending);
    }

    public class ParcelFilterV2
    {
        public string Status { get; set; }
        public string AddressId { get; set; }
    }
}