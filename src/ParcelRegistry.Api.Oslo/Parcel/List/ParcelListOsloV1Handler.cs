namespace ParcelRegistry.Api.Oslo.Parcel.List
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParcelListOsloV1Handler : IRequestHandler<ParcelListOsloRequest, ParcelListOsloResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ParcelListOsloV1Handler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelListOsloResponse> Handle(ParcelListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagedParcels = new ParcelListOsloQuery(_context, _syndicationContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var parcelListItemOsloResponses = await pagedParcels.Items
                .Select(m => new ParcelListItemOsloResponse(
                    m.PersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    m.Status.MapToPerceelStatus(),
                    m.VersionTimestamp.ToBelgianDateTimeOffset()))
                .ToListAsync(cancellationToken);

            return new ParcelListOsloResponse
            {
                Context = _responseOptions.Value.ContextUrlList,
                Percelen = parcelListItemOsloResponses,
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(parcelListItemOsloResponses.Count, _responseOptions.Value.VolgendeUrl),
                Sorting = pagedParcels.Sorting,
                Pagination = pagedParcels.PaginationInfo
            };
        }
    }
}
