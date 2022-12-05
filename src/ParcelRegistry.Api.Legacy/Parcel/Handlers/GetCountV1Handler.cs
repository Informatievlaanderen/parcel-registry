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
    using Projections.Syndication;
    using Query;
    using Requests;

    public class GetCountV1Handler : IRequestHandler<GetCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        public GetCountV1Handler(LegacyContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        public async Task<TotaalAantalResponse> Handle(GetCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
            {
                Aantal = filtering.ShouldFilter
                    ? await new ParcelListQuery(_context, _syndicationContext)
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
