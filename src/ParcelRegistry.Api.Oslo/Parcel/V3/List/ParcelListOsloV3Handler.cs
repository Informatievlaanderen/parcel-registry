namespace ParcelRegistry.Api.Oslo.Parcel.V3.List
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParcelListOsloV3Handler : IRequestHandler<ParcelListOsloRequest, ParcelListOsloV3Response>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public ParcelListOsloV3Handler(
            LegacyContext context,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelListOsloV3Response> Handle(ParcelListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagedParcels = new ParcelListOsloV3Query(_context)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var parcelListItemOsloResponses = await pagedParcels.Items
                .Select(m => new ParcelListItemOsloV3Response(
                    m.CaPaKey,
                    _responseOptions.Value.DetailUrl,
                    m.Status.MapToOsloPerceelStatus(),
                    m.VersionTimestamp.ToBelgianDateTimeOffset()))
                .ToListAsync(cancellationToken);

            return new ParcelListOsloV3Response
            {
                Context = _responseOptions.Value.ContextUrlList,
                Percelen = parcelListItemOsloResponses,
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(parcelListItemOsloResponses.Count, _responseOptions.Value.VolgendeUrl),
                Sorting = pagedParcels.Sorting,
                Pagination = pagedParcels.PaginationInfo
            };
        }
    }
}
