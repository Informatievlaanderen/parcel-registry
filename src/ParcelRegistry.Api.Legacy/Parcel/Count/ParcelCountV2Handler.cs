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

    public class ParcelCountV2Handler : IRequestHandler<ParcelCountRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public ParcelCountV2Handler(LegacyContext context)
        {
            _context = context;
        }

        public async Task<TotaalAantalResponse> Handle(ParcelCountRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.Filtering.ShouldFilter
                    ? await new ParcelListV2Query(_context)
                        .Fetch(request.Filtering, request.Sorting, request.Pagination)
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
