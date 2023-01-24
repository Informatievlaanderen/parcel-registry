namespace ParcelRegistry.Api.Legacy.Parcel.Count
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using ParcelRegistry.Projections.Legacy;
    using Projections.Syndication;

    public class ParcelCountV1Handler : IRequestHandler<ParcelCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        public ParcelCountV1Handler(LegacyContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        public async Task<TotaalAantalResponse> Handle(ParcelCountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.Filtering.ShouldFilter
                    ? await new ParcelListQuery(_context, _syndicationContext)
                        .Fetch(request.Filtering, request.Sorting, request.Pagination)
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
