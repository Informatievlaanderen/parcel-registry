using MediatR;

namespace ParcelRegistry.Api.Legacy.Parcel.Detail
{
    public record ParcelDetailRequest(string CaPaKey) : IRequest<ParcelResponseWithEtag>;
}
