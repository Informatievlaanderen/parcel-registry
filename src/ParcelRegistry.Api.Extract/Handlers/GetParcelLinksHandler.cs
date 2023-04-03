namespace ParcelRegistry.Api.Extract.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Extracts;
    using MediatR;
    using Projections.Extract;

    public class GetParcelLinksHandler : IRequestHandler<GetParcelLinksRequest, IsolationExtractArchive>
    {
        private readonly ExtractContext _context;

        public GetParcelLinksHandler(ExtractContext context)
        {
            _context = context;
        }

        public Task<IsolationExtractArchive> Handle(GetParcelLinksRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new IsolationExtractArchive(ExtractFileNames.ParcelLinkExtractFileName, _context)
            {
                ParcelRegistryLinkExtractBuilder.CreateParcelFiles(_context)
            });
        }
    }
}
