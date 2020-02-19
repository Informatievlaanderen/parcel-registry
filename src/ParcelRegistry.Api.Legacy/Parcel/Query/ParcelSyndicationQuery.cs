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
        public bool ContainsEvent { get; }
        public bool ContainsObject { get; }

        public Guid ParcelId { get; }
        public long Position { get; }
        public string CaPaKey { get; }
        public string ChangeType { get; }
        public Instant RecordCreatedAt { get; }
        public Instant LastChangedOn { get; }
        public ParcelStatus? Status { get; }
        public IEnumerable<Guid> AddressIds { get; } = new List<Guid>();
        public bool IsComplete { get; }
        public Organisation? Organisation { get; }
        public string Reason { get; }
        public string EventDataAsXml { get; }

        public ParcelSyndicationQueryResult(
            Guid addressId,
            long position,
            string caPaKey,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            Organisation? organisation,
            string reason)
        {
            ContainsObject = false;
            ContainsEvent = false;

            ParcelId = addressId;
            Position = position;
            CaPaKey = caPaKey;
            ChangeType = changeType;
            RecordCreatedAt = recordCreateAt;
            LastChangedOn = lastChangedOn;
            Organisation = organisation;
            Reason = reason;
        }

        public ParcelSyndicationQueryResult(
            Guid addressId,
            long position,
            string caPaKey,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            Organisation? organisation,
            string reason,
            string eventDataAsXml)
            : this(
                addressId,
                position,
                caPaKey,
                changeType,
                recordCreateAt,
                lastChangedOn,
                organisation,
                reason)
        {
            ContainsEvent = true;

            EventDataAsXml = eventDataAsXml;
        }

        public ParcelSyndicationQueryResult(
            Guid addressId,
            long position,
            string caPaKey,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            ParcelStatus? status,
            IEnumerable<Guid> addressIds,
            Organisation? organisation,
            string reason)
            : this(
                addressId,
                position,
                caPaKey,
                changeType,
                recordCreateAt,
                lastChangedOn,
                organisation,
                reason)
        {
            ContainsObject = true;

            IsComplete = isComplete;
            Status = status;
            AddressIds = addressIds;
        }

        public ParcelSyndicationQueryResult(
            Guid addressId,
            long position,
            string caPaKey,
            string changeType,
            Instant recordCreateAt,
            Instant lastChangedOn,
            bool isComplete,
            ParcelStatus? status,
            IEnumerable<Guid> addressIds,
            Organisation? organisation,
            string reason,
            string eventDataAsXml)
            : this(
                addressId,
                position,
                caPaKey,
                changeType,
                recordCreateAt,
                lastChangedOn,
                isComplete,
                status,
                addressIds,
                organisation,
                reason)
        {
            ContainsEvent = true;

            EventDataAsXml = eventDataAsXml;
        }
    }

    public class ParcelSyndicationQuery : Query<ParcelSyndicationItem, ParcelSyndicationFilter, ParcelSyndicationQueryResult>
    {
        private readonly LegacyContext _context;
        private readonly bool _embedEvent;
        private readonly bool _embedObject;

        public ParcelSyndicationQuery(LegacyContext context, bool embedEvent, bool embedObject)
        {
            _context = context;
            _embedEvent = embedEvent;
            _embedObject = embedObject;
        }

        protected override ISorting Sorting => new ParcelSyndicationSorting();

        protected override Expression<Func<ParcelSyndicationItem, ParcelSyndicationQueryResult>> Transformation
        {
            get
            {
                if (_embedObject && _embedEvent)
                    return x => new ParcelSyndicationQueryResult(
                        x.ParcelId.Value,
                        x.Position,
                        x.CaPaKey,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.IsComplete,
                        x.Status,
                        x.AddressIds,
                        x.Organisation,
                        x.Reason,
                        x.EventDataAsXml);

                if (_embedEvent)
                    return x => new ParcelSyndicationQueryResult(
                        x.ParcelId.Value,
                        x.Position,
                        x.CaPaKey,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.Organisation,
                        x.Reason,
                        x.EventDataAsXml);

                if (_embedObject)
                    return x => new ParcelSyndicationQueryResult(
                        x.ParcelId.Value,
                        x.Position,
                        x.CaPaKey,
                        x.ChangeType,
                        x.RecordCreatedAt,
                        x.LastChangedOn,
                        x.IsComplete,
                        x.Status,
                        x.AddressIds,
                        x.Organisation,
                        x.Reason);

                return x => new ParcelSyndicationQueryResult(
                    x.ParcelId.Value,
                    x.Position,
                    x.CaPaKey,
                    x.ChangeType,
                    x.RecordCreatedAt,
                    x.LastChangedOn,
                    x.Organisation,
                    x.Reason);
            }
        }

        protected override IQueryable<ParcelSyndicationItem> Filter(FilteringHeader<ParcelSyndicationFilter> filtering)
        {
            var parcels = _context
                .ParcelSyndication
                .OrderBy(x => x.Position)
                .AsNoTracking();

            if (!filtering.ShouldFilter)
                return parcels;

            if (filtering.Filter.Position.HasValue)
                parcels = parcels.Where(m => m.Position >= filtering.Filter.Position);

            return parcels;
        }
    }

    public class ParcelSyndicationSorting : ISorting
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
        public string Embed { get; set; }

        public bool ContainsEvent =>
            Embed.Contains("event", StringComparison.OrdinalIgnoreCase);

        public bool ContainsObject =>
            Embed.Contains("object", StringComparison.OrdinalIgnoreCase);
    }
}
