namespace ParcelRegistry.Api.Oslo.Parcel.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using List;
    using ParcelRegistry.Projections.Legacy;
    using Projections.Syndication;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParcelCountOsloV1Handler : IRequestHandler<ParcelCountOsloRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;

        public ParcelCountOsloV1Handler(LegacyContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
        }

        public async Task<TotaalAantalResponse> Handle(ParcelCountOsloRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.Filtering.ShouldFilter
                        ? await new ParcelListOsloQuery(_context, _syndicationContext)
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
