namespace ParcelRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;

    public class ParcelLastChangedListRunner : LastChangedListRunner
    {
        public const string Name = "ParcelLastChangedListRunner";

        public ParcelLastChangedListRunner(
            EnvelopeFactory envelopeFactory,
            ILogger<ParcelLastChangedListRunner> logger) :
            base(
                Name,
                new Projections(),
                envelopeFactory,
                logger) { }
    }
}
