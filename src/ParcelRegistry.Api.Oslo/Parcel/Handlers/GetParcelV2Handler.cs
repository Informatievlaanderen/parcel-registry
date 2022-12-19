namespace ParcelRegistry.Api.Oslo.Parcel.Handlers
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
    using Requests;
    using Responses;

    public class GetParcelV2Handler : IRequestHandler<GetParcelRequest, ParcelOsloResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public GetParcelV2Handler(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelOsloResponseWithEtag> Handle(GetParcelRequest request, CancellationToken cancellationToken)
        {
            var parcel =
                await _context
                    .ParcelDetailV2
                    .Include(x => x.Addresses)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.CaPaKey == request.CaPaKey, cancellationToken);

            if (parcel is not null && parcel.Removed)
                throw new ApiException("Perceel werd verwijderd.", StatusCodes.Status410Gone);

            if (parcel is null)
                throw new ApiException("Onbestaand perceel.", StatusCodes.Status404NotFound);

            var response = new ParcelOsloResponse(
                _responseOptions.Value.Naamruimte,
                _responseOptions.Value.ContextUrlDetail,
                parcel.Status.MapToPerceelStatus(),
                parcel.CaPaKey,
                parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                parcel.Addresses
                    .Select(x => x.AddressPersistentLocalId.ToString())
                    .OrderBy(x => x)
                    .ToList(),
                _responseOptions.Value.AdresDetailUrl);

            return new ParcelOsloResponseWithEtag(response, parcel.LastEventHash);
        }
    }
}
