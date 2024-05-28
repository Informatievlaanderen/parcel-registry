namespace ParcelRegistry.Api.Legacy.Parcel.Detail
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
    using Projections.Legacy;

    public class ParcelDetailV2Handler : IRequestHandler<ParcelDetailRequest, ParcelResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ParcelDetailV2Handler(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelResponseWithEtag> Handle(ParcelDetailRequest request, CancellationToken cancellationToken)
        {
            var parcel =
                await _context
                    .ParcelDetailWithCountV2
                    .Include(x => x.Addresses)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.CaPaKey == request.CaPaKey, cancellationToken);

            if (parcel is null)
            {
                throw new ApiException("Onbestaand perceel.", StatusCodes.Status404NotFound);
            }

            if (parcel is { Removed: true })
            {
                throw new ApiException("Perceel werd verwijderd.", StatusCodes.Status410Gone);
            }

            var response = new ParcelDetailResponse(
                _responseOptions.Value.Naamruimte,
                parcel.Status.MapToPerceelStatus(),
                parcel.CaPaKey,
                parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                parcel.Addresses
                    .Select(x => x.AddressPersistentLocalId.ToString())
                    .OrderBy(x => x)
                    .ToList(),
                _responseOptions.Value.AdresDetailUrl);

            return new ParcelResponseWithEtag(response, parcel.LastEventHash);
        }
    }
}
