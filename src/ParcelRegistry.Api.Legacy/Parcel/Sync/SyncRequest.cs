namespace ParcelRegistry.Api.Legacy.Parcel.Sync
{
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record SyncRequest(HttpRequest HttpRequest) : IRequest<string>;
}
