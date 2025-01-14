namespace ParcelRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class ParcelProducer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<ParcelProducer> _logger;
        private readonly IConnectedProjectionsManager _projectionManager;

        public ParcelProducer(
            IHostApplicationLifetime hostApplicationLifetime,
            IConnectedProjectionsManager projectionManager,
            ILogger<ParcelProducer> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _projectionManager = projectionManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _projectionManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(ParcelProducer)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
