namespace ParcelRegistry.Api.Extract.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Extracts;
    using MediatR;
    using ParcelRegistry.Projections.Extract;

    public class GetParcelsV1Handler : IRequestHandler<GetParcelsRequest, IsolationExtractArchive>
    {
        private readonly ExtractContext _context;

        public GetParcelsV1Handler(ExtractContext context)
        {
            _context = context;
        }

        public Task<IsolationExtractArchive> Handle(GetParcelsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.ParcelExtractFileName, _context)
            {
                ParcelRegistryExtractBuilder.CreateParcelFiles(_context)
            });
        }
    }
}
