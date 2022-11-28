namespace ParcelRegistry.Api.Oslo.Parcel.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record GetParcelCountRequest(
        HttpRequest HttpRequest) : IRequest<TotaalAantalResponse>;
}
