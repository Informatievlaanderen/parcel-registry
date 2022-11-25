namespace ParcelRegistry.Api.Extract.Handlers
{
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Extracts;
    using MediatR;
    using ParcelRegistry.Projections.Extract;

    public class GetParcelsV2Handler : RequestHandler<GetParcelsRequest, IsolationExtractArchive>
    {
        private readonly ExtractContext _context;

        public GetParcelsV2Handler(ExtractContext context)
        {
            _context = context;
        }

        protected override IsolationExtractArchive Handle(GetParcelsRequest request)
        {
            return new IsolationExtractArchive(ExtractFileNames.FileName, _context)
            {
                ParcelRegistryExtractV2Builder.CreateParcelFiles(_context)
            };
        }
    }
}
