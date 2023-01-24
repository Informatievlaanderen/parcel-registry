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
    using Projections.Syndication;

    public class ParcelListV1Handler : IRequestHandler<ParcelListRequest, ParcelListResponse?>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ParcelListV1Handler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }
        public async Task<ParcelListResponse?> Handle(ParcelListRequest request, CancellationToken cancellationToken)
        {
            var pagedParcels = new ParcelListQuery(_context, _syndicationContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            return new ParcelListResponse
            {
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemResponse(
                        m.PersistentLocalId,
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
