namespace ParcelRegistry.Api.Legacy.Parcel.Handlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Requests;
    using Projections.Legacy;
    using Projections.Syndication;
    using Responses;

    public class GetParcelV1Handler : IRequestHandler<GetParcelRequest, ParcelResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetParcelV1Handler(
            LegacyContext context,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelResponseWithEtag> Handle(GetParcelRequest request, CancellationToken cancellationToken)
        {
            var parcel =
                await _context
                    .ParcelDetail
                    .Include(x => x.Addresses)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.PersistentLocalId == request.CaPaKey, cancellationToken);

            if (parcel is not null && parcel.Removed)
            {
                throw new ApiException("Perceel werd verwijderd.", StatusCodes.Status410Gone);
            }

            if (parcel is null)
            {
                throw new ApiException("Onbestaand perceel.", StatusCodes.Status404NotFound);
            }

            var addressIds = parcel.Addresses.Select(x => x.AddressId);

            var addressPersistentLocalIds = await _syndicationContext
                .AddressPersistentLocalIds
                .AsNoTracking()
                .Where(x => addressIds.Contains(x.AddressId) && x.IsComplete && !x.IsRemoved)
                .Select(x => x.PersistentLocalId)
                .OrderBy(x => x) //sorts on string! other order as a number!
                .ToListAsync(cancellationToken);

            return new ParcelResponseWithEtag(new ParcelResponse(
                _responseOptions.Value.Naamruimte,
                parcel.Status.MapToPerceelStatus(),
                parcel.PersistentLocalId,
                parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                addressPersistentLocalIds.ToList(),
                _responseOptions.Value.AdresDetailUrl));
        }
    }
}
