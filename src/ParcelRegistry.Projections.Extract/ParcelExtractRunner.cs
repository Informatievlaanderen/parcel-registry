namespace ParcelRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Logging;
    using ParcelExtract;

    public class ParcelExtractRunner : Runner<ExtractContext>
    {
        public const string Name = "ParcelExtractRunner";

        public ParcelExtractRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<ParcelExtractRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new ParcelExtractProjections(DbaseCodePage.Western_European_ANSI.ToEncoding())) { }
    }
}
