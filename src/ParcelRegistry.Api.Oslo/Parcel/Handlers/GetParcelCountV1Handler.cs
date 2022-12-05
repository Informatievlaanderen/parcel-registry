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
    using Projections.Syndication;
    using Requests;

    public class GetParcelCountV1Handler : IRequestHandler<GetParcelCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        public GetParcelCountV1Handler(LegacyContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        public async Task<TotaalAantalResponse> Handle(GetParcelCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return new TotaalAantalResponse
            {
                Aantal = filtering.ShouldFilter
                        ? await new ParcelListOsloQuery(_context, _syndicationContext)
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
