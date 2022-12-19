namespace ParcelRegistry.Api.Oslo.Parcel.Requests
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Responses;

    public record GetParcelRequest(
        string CaPaKey) : IRequest<ParcelOsloResponseWithEtag>;
}
