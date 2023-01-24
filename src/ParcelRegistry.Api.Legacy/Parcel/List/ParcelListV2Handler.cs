namespace ParcelRegistry.Api.Legacy.Parcel.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Convertors;
    using Infrastructure;
    using Infrastructure.Options;
    using ParcelRegistry.Projections.Legacy;

    public class ParcelListV2Handler : IRequestHandler<ParcelListRequest, ParcelListResponse?>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _reponseOptions;

        public ParcelListV2Handler(
            LegacyContext context,
            IOptions<ResponseOptions> reponseOptions)
        {
            _context = context;
            _reponseOptions = reponseOptions;
        }
        public async Task<ParcelListResponse?> Handle(ParcelListRequest request, CancellationToken cancellationToken)
        {
            var pagedParcels = new ParcelListV2Query(_context)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            return new ParcelListResponse
            {
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemResponse(
                        m.CaPaKey,
                        _reponseOptions.Value.Naamruimte,
                        _reponseOptions.Value.DetailUrl,
                        m.Status.MapToPerceelStatus(),
                        m.VersionTimestamp.ToBelgianDateTimeOffset()))
                    .ToListAsync(cancellationToken),
                Volgende = pagedParcels.PaginationInfo.BuildVolgendeUri(_reponseOptions.Value.VolgendeUrl),
                Sorting = pagedParcels.Sorting,
                Pagination = pagedParcels.PaginationInfo
            };
        }
    }
}
