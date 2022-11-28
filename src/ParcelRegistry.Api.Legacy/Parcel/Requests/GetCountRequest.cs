namespace ParcelRegistry.Api.Legacy.Parcel.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record GetCountRequest(HttpRequest HttpRequest) : IRequest<TotaalAantalResponse>, IRequest<Unit>;
}
