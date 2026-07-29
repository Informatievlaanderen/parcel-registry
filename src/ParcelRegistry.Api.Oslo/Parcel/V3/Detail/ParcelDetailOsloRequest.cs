namespace ParcelRegistry.Api.Oslo.Parcel.V3.Detail
{
    using MediatR;

    public record ParcelDetailOsloRequest(string CaPaKey) : IRequest<ParcelDetailOsloV3ResponseWithEtag>;
}
