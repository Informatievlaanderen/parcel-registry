namespace ParcelRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;
    using ParcelDetail;

    public class ParcelLegacyRunner : Runner<LegacyContext>
    {
        public const string Name = "ParcelLegacyRunner";

        public ParcelLegacyRunner(EnvelopeFactory envelopeFactory,
            ILogger<ParcelLegacyRunner> logger) :
            base(
                Name,
                envelopeFactory,
                logger,
                new ParcelDetailProjections()) { }
    }
}
