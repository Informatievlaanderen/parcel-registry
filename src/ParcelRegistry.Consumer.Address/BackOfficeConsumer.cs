namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public sealed class BackOfficeConsumer : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IIdempotentConsumer<ConsumerAddressContext> _kafkaIdemIdompotencyConsumer;
        private readonly ILogger<BackOfficeConsumer> _logger;

        public BackOfficeConsumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<ConsumerAddressContext> kafkaIdemIdompotencyConsumer)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _backOfficeContextFactory = backOfficeContextFactory;
            _loggerFactory = loggerFactory;
            _kafkaIdemIdompotencyConsumer = kafkaIdemIdompotencyConsumer;

            _logger = loggerFactory.CreateLogger<BackOfficeConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var backOfficeProjector =
                new ConnectedProjector<ConsumerAddressContext>(
                    Resolve.WhenEqualToHandlerMessageType(new BackOfficeKafkaProjection().Handlers));

            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(
                    new CommandHandlingKafkaProjection(_backOfficeContextFactory).Handlers));

            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);

            try
            {
                await _kafkaIdemIdompotencyConsumer.ConsumeContinuously(async (message, context) =>
                {
                    _logger.LogInformation("Handling next message");

                    await commandHandlingProjector.ProjectAsync(commandHandler, message, stoppingToken).ConfigureAwait(false);
                    await backOfficeProjector.ProjectAsync(context, message, stoppingToken).ConfigureAwait(false);

                    //CancellationToken.None to prevent halfway consumption
                    await context.SaveChangesAsync(CancellationToken.None);

                }, stoppingToken);
            }
            catch (Exception)
            {
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}
