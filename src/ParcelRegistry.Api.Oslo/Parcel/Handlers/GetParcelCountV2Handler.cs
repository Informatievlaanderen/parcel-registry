namespace ParcelRegistry.Api.Oslo.Parcel.Handlers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Query;
    using ParcelRegistry.Projections.Legacy;
    using Requests;

    public class GetParcelCountV2Handler : IRequestHandler<GetParcelCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public GetParcelCountV2Handler(LegacyContext context)
        {
            _context = context;
        }

        public async Task<TotaalAantalResponse> Handle(GetParcelCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilterV2>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
            {
                Aantal = filtering.ShouldFilter
                        ? await new ParcelListOsloV2Query(_context)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32(_context
                            .ParcelDetailV2ListViewCount
                            .First()
                            .Count)
            };
        }
    }
}
