namespace ParcelRegistry.Api.Extract.Handlers
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Extracts;
    using MediatR;
    using ParcelRegistry.Projections.Extract;

    public class GetParcelsHandler : RequestHandler<GetParcelsRequest, IsolationExtractArchive>
    {
        private readonly ExtractContext _context;

        public GetParcelsHandler(ExtractContext context)
        {
            _context = context;
        }

        protected override IsolationExtractArchive Handle(GetParcelsRequest request)
        {
            return new IsolationExtractArchive(ExtractFileNames.FileName, _context)
            {
                ParcelRegistryExtractBuilder.CreateParcelFiles(_context)
            };
        }
    }
}
