namespace ParcelRegistry.Api.Extract.Handlers
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using MediatR;

    public struct GetParcelsRequest : IRequest<IsolationExtractArchive>
    { }

    public struct GetParcelLinksRequest : IRequest<IsolationExtractArchive>
    { }
}
