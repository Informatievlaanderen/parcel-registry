namespace ParcelRegistry.Api.Legacy.Parcel.Handlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Query;
    using ParcelRegistry.Projections.Legacy;
    using Projections.Syndication;
    using Responses;
    using Requests;

    public class GetParcelListV1Handler : IRequestHandler<GetParcelListRequest, ParcelListResponse?>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetParcelListV1Handler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }
        public async Task<ParcelListResponse?> Handle(GetParcelListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedParcels = new ParcelListQuery(_context, _syndicationContext)
                .Fetch(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedParcels);

            var response = new ParcelListResponse
            {
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemResponse(
                        m.PersistentLocalId,
                        _responseOptions.Value.Naamruimte,
                        _responseOptions.Value.DetailUrl,
                        m.Status.MapToPerceelStatus(),
                        m.VersionTimestamp.ToBelgianDateTimeOffset()))
                    .ToListAsync(cancellationToken),
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(_responseOptions.Value.VolgendeUrl)
            };

            return response;
        }
    }
}
