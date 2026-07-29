namespace ParcelRegistry.Api.Oslo.Parcel.V3.List
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
    using Projections.Legacy.ParcelDetail;

    public class ParcelListOsloV3Query : Query<ParcelListV3QueryItem, ParcelFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new ParcelSortingV3();

        public ParcelListOsloV3Query(LegacyContext context) => _context = context;

        protected override IQueryable<ParcelListV3QueryItem> Filter(FilteringHeader<ParcelFilter> filtering)
        {
            var parcels = _context
                .ParcelDetails
                .AsNoTracking()
                .OrderBy(x => x.CaPaKey)
                .Where(x => !x.Removed);

            if (!filtering.ShouldFilter)
            {
                return parcels.Select(x => new ParcelListV3QueryItem
                {
                    CaPaKey = x.CaPaKey,
                    StatusAsString = x.StatusAsString,
                    VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
                });
            }

            if (!string.IsNullOrEmpty(filtering.Filter.AddressId))
            {
                if (int.TryParse(filtering.Filter.AddressId, out var addressId))
                {
                    parcels = parcels.Where(x => x.Addresses.Any(parcelDetailAddress => parcelDetailAddress.AddressPersistentLocalId == addressId));
                }
                else
                {
                    return new List<ParcelListV3QueryItem>().AsQueryable();
                }
            }

            if (!string.IsNullOrEmpty(filtering.Filter.Status))
            {
                if (Enum.TryParse(typeof(PerceelStatus), filtering.Filter.Status, true, out var status))
                {
                    var parcelStatus = ((PerceelStatus)status).MapToParcelStatus();
                    parcels = parcels.Where(m => m.StatusAsString == parcelStatus.Status);
                }
                else
                {
                    return new List<ParcelListV3QueryItem>().AsQueryable();
                }
            }

            return parcels.Select(x => new ParcelListV3QueryItem
            {
                CaPaKey = x.CaPaKey,
                StatusAsString = x.StatusAsString,
                VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
            });
        }
    }

    public class ParcelFilter
    {
        public string Status { get; set; }
        public string AddressId { get; set; }
    }

    public class ParcelSortingV3 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(ParcelDetail.CaPaKey),
        };

        public SortingHeader DefaultSortingHeader { get; } =
            new SortingHeader(nameof(ParcelDetail.CaPaKey), SortOrder.Ascending);
    }
}
