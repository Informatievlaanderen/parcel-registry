namespace ParcelRegistry.Api.Extract.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Extracts;
    using MediatR;
    using ParcelRegistry.Projections.Extract;

    public class GetParcelsV2Handler : IRequestHandler<GetParcelsRequest, IsolationExtractArchive>
    {
        private readonly ExtractContext _context;

        public GetParcelsV2Handler(ExtractContext context)
        {
            _context = context;
        }

        public Task<IsolationExtractArchive> Handle(GetParcelsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.FileName, _context)
            {
                ParcelRegistryExtractV2Builder.CreateParcelFiles(_context)
            });
        }
    }
}
