namespace ParcelRegistry.Api.Oslo.Parcel.Sync
{
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record SyncRequest(HttpRequest HttpRequest) : IRequest<string>;
}
