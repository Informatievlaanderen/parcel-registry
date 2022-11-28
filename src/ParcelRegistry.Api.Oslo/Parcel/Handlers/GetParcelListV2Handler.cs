namespace ParcelRegistry.Api.Oslo.Parcel.Handlers
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
    using Projections.Legacy;
    using Query;
    using Requests;
    using Responses;

    public class GetParcelListV2Handler : IRequestHandler<GetParcelListRequest, ParcelListOsloResponse>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetParcelListV2Handler(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelListOsloResponse> Handle(GetParcelListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilterV2>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedParcels = new ParcelListOsloV2Query(_context)
                .Fetch(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedParcels);

            return new ParcelListOsloResponse
            {
                Context = _responseOptions.Value.ContextUrlList,
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemOsloResponse(
                        m.CaPaKey,
                        _responseOptions.Value.Naamruimte,
                        _responseOptions.Value.DetailUrl,
                        m.Status.MapToPerceelStatus(),
                        m.VersionTimestamp.ToBelgianDateTimeOffset()))
                    .ToListAsync(cancellationToken),
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(_responseOptions.Value.VolgendeUrl)
            };
        }
    }
}
