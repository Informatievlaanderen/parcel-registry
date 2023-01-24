namespace ParcelRegistry.Api.Oslo.Parcel.Detail
{
    using MediatR;

    public record ParcelDetailOsloRequest(string CaPaKey) : IRequest<ParcelDetailOsloResponseWithEtag>;
}
