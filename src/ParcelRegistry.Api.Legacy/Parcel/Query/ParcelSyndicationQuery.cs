namespace ParcelRegistry.Api.Legacy.Parcel.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.ParcelSyndication;

    public class ParcelSyndicationQueryResult
    {
        public bool ContainsDetails { get; }

        public Guid ParcelId { get; }
        public long Position { get; }
        public string CaPaKey { get; }
        public string ChangeType { get; }
        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public ParcelStatus? Status { get; }
        public List<Guid> AddressIds { get; }
        public bool IsComplete { get; }
        public Organisation? Organisation { get; }
        public Plan? Plan { get; }

        public ParcelSyndicationQueryResult(
            Guid addressId,
            long position,
            string caPaKey,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            ParcelStatus? status,
            List<Guid> addressIds,
            Organisation? organisation,
            Plan? plan)
        {
            ContainsDetails = false;

            ParcelId = addressId;
            Position = position;
            CaPaKey = caPaKey;
            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
            IsComplete = isComplete;
            Status = status;
            AddressIds = addressIds;
            Organisation = organisation;
            Plan = plan;
        }
    }

    public class ParcelSyndicationQuery : Query<ParcelSyndicationItem, ParcelSyndicationFilter, ParcelSyndicationQueryResult>
    {
        private readonly LegacyContext _context;

        public ParcelSyndicationQuery(LegacyContext context)
        {
            _context = context;
        }

        protected override ISorting Sorting => new ParcelSyndicationSorting();

        protected override Expression<Func<ParcelSyndicationItem, ParcelSyndicationQueryResult>> Transformation =>
            x => new ParcelSyndicationQueryResult(
                x.ParcelId.Value,
                x.Position,
                x.CaPaKey,
                x.ChangeType,
                x.RecordCreatedAt,
                x.LastChangedOn,
                x.IsComplete,
                x.Status,
                x.AddressIds.ToList(),
                x.Organisation,
                x.Plan);

        protected override IQueryable<ParcelSyndicationItem> Filter(FilteringHeader<ParcelSyndicationFilter> filtering)
        {
            var parcels = _context
                .ParcelSyndication
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return parcels;

            if (filtering.Filter.Position.HasValue)
                parcels = parcels.Where(m => m.Position >= filtering.Filter.Position);

            return parcels;
        }
    }

    internal class ParcelSyndicationSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(ParcelSyndicationItem.Position)
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(ParcelSyndicationItem.Position), SortOrder.Ascending);
    }

    public class ParcelSyndicationFilter
    {
        public long? Position { get; set; }
    }
}
