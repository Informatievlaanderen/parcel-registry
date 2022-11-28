namespace ParcelRegistry.Api.Legacy.Parcel.Handlers
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
    using Projections.Legacy;
    using Query;
    using Requests;

    public class GetCountV1Handler : IRequestHandler<GetCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public GetCountV1Handler(LegacyContext context)
        {
            _context = context;
        }

        public async Task<TotaalAantalResponse> Handle(GetCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
            {
                Aantal = filtering.ShouldFilter
                    ? await new ParcelListQuery(_context)
                        .Fetch(filtering, sorting, pagination)
                        .Items
                        .CountAsync(cancellationToken)
                    : Convert.ToInt32(_context
                        .ParcelDetailListViewCount
                        .First()
                        .Count)
            };
        }
    }
}
