namespace ParcelRegistry.Api.Oslo.Parcel.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using ParcelRegistry.Projections.Legacy;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParcelCountOsloV2Handler : IRequestHandler<ParcelCountOsloRequest, TotaalAantalResponse>
    {
        private readonly LegacyContext _context;

        public ParcelCountOsloV2Handler(LegacyContext context)
        {
            _context = context;
        }

        public async Task<TotaalAantalResponse> Handle(ParcelCountOsloRequest request, CancellationToken cancellationToken)
        {
            return new TotaalAantalResponse
            {
                Aantal = request.Filtering.ShouldFilter
                        ? await new ParcelListOsloV2Query(_context)
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
