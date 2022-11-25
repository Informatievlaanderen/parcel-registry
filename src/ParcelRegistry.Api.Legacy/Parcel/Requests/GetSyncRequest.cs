namespace ParcelRegistry.Api.Legacy.Parcel.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record GetSyncRequest(HttpRequest HttpRequest) : IRequest<string>;
}
