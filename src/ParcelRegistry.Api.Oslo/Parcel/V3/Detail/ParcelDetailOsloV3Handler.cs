namespace ParcelRegistry.Api.Oslo.Parcel.V3.Detail
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParcelDetailOsloV3Handler : IRequestHandler<ParcelDetailOsloRequest, ParcelDetailOsloV3ResponseWithEtag>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptionsV3> _responseOptions;

        public ParcelDetailOsloV3Handler(
            LegacyContext context,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<ParcelDetailOsloV3ResponseWithEtag> Handle(ParcelDetailOsloRequest request, CancellationToken cancellationToken)
        {
            var parcel =
                await _context
                    .ParcelDetails
                    .Include(x => x.Addresses)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.CaPaKey == request.CaPaKey, cancellationToken);

            if (parcel is not null && parcel.Removed)
                throw new ApiException("Perceel werd verwijderd.", StatusCodes.Status410Gone);

            if (parcel is null)
                throw new ApiException("Onbestaand perceel.", StatusCodes.Status404NotFound);

            var response = new ParcelDetailOsloV3Response(
                _responseOptions.Value.ContextUrlDetail,
                parcel.Status.MapToOsloPerceelStatus(),
                parcel.CaPaKey,
                parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                parcel.Addresses
                    .Select(x => x.AddressPersistentLocalId.ToString())
                    .OrderBy(x => x)
                    .ToList(),
                _responseOptions.Value.AdresDetailUrl,
                _responseOptions.Value.DetailUrl,
                _responseOptions.Value.ParcelDetailBuildingsLink);

            return new ParcelDetailOsloV3ResponseWithEtag(response, parcel.LastEventHash);
        }
    }
}
