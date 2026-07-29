namespace ParcelRegistry.Api.Oslo.Parcel.V2.Detail
{
    using MediatR;

    public record ParcelDetailOsloRequest(string CaPaKey) : IRequest<ParcelDetailOsloResponseWithEtag>;
}
