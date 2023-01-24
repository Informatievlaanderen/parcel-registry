namespace ParcelRegistry.Api.Oslo.Parcel.List
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

    public class ParcelListOsloV2Handler : IRequestHandler<ParcelListOsloRequest, ParcelListOsloResponse>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ParcelListOsloV2Handler(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelListOsloResponse> Handle(ParcelListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagedParcels = new ParcelListOsloV2Query(_context)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

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
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(_responseOptions.Value.VolgendeUrl),
                Sorting = pagedParcels.Sorting,
                Pagination = pagedParcels.PaginationInfo
            };
        }
    }
}
