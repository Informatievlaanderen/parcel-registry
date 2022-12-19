using MediatR;
using ParcelRegistry.Api.Legacy.Parcel.Responses;

namespace ParcelRegistry.Api.Legacy.Parcel.Requests
{
    public record GetParcelRequest(string CaPaKey) : IRequest<ParcelResponseWithEtag>;
}
