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
    using Requests;
    using ParcelRegistry.Projections.Legacy;
    using Query;
    using Responses;

    public class GetParcelListV2Handler : IRequestHandler<GetParcelListRequest, ParcelListResponse?>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _reponseOptions;

        public GetParcelListV2Handler(
            LegacyContext context,
            IOptions<ResponseOptions> reponseOptions)
        {
            _context = context;
            _reponseOptions = reponseOptions;
        }
        public async Task<ParcelListResponse?> Handle(GetParcelListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilterV2>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedParcels = new ParcelListV2Query(_context)
                .Fetch(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedParcels);

            var response = new ParcelListResponse
            {
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemResponse(
                        m.CaPaKey,
                        _reponseOptions.Value.Naamruimte,
                        _reponseOptions.Value.DetailUrl,
                        m.Status.MapToPerceelStatus(),
                        m.VersionTimestamp.ToBelgianDateTimeOffset()))
                    .ToListAsync(cancellationToken),
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(_reponseOptions.Value.VolgendeUrl)
            };

            return response;
        }
    }
}
