namespace ParcelRegistry.Api.Oslo.Parcel.Requests
{
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Responses;

    public record GetParcelListRequest(
        HttpRequest HttpRequest,
        HttpResponse HttpResponse) : IRequest<ParcelListOsloResponse>;
}
