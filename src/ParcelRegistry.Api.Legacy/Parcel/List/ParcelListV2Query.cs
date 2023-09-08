namespace ParcelRegistry.Api.Legacy.Parcel.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Microsoft.EntityFrameworkCore;
    using Convertors;
    using ParcelRegistry.Projections.Legacy;
    using ParcelRegistry.Projections.Legacy.ParcelDetailV2;

    public class ParcelListV2Query : Query<ParcelListV2QueryItem, ParcelFilter>
    {
        private readonly LegacyContext _context;

        protected override ISorting Sorting => new ParcelSortingV2();

        public ParcelListV2Query(LegacyContext context) => _context = context;

        protected override IQueryable<ParcelListV2QueryItem> Filter(FilteringHeader<ParcelFilter> filtering)
        {
            var parcels = _context
                .ParcelDetailV2
                .AsNoTracking()
                .OrderBy(x => x.CaPaKey)
                .Where(x => !x.Removed);

            if (!filtering.ShouldFilter)
            {
                return parcels
                    .Select(x => new ParcelListV2QueryItem
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
                    return new List<ParcelListV2QueryItem>().AsQueryable();
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
                    return new List<ParcelListV2QueryItem>().AsQueryable();
                }
            }

            return parcels
                .Select(x => new ParcelListV2QueryItem
                {
                    CaPaKey = x.CaPaKey,
                    StatusAsString = x.StatusAsString,
                    VersionTimestampAsDateTimeOffset = x.VersionTimestampAsDateTimeOffset
                });;
        }
    }

    public class ParcelSortingV2 : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(ParcelDetailV2.CaPaKey),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(ParcelDetailV2.CaPaKey), SortOrder.Ascending);
    }
}
