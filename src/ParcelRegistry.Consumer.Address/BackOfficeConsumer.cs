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
    using Parcel;
    using Projections;

    public sealed class BackOfficeConsumer : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;
        private readonly IParcels _parcels;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IIdempotentConsumer<ConsumerAddressContext> _kafkaIdompotencyConsumer;
        private readonly ILogger<BackOfficeConsumer> _logger;

        public BackOfficeConsumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            IParcels parcels,
            ILoggerFactory loggerFactory,
            IIdempotentConsumer<ConsumerAddressContext> kafkaIdompotencyConsumer)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _backOfficeContextFactory = backOfficeContextFactory;
            _parcels = parcels;
            _loggerFactory = loggerFactory;
            _kafkaIdompotencyConsumer = kafkaIdompotencyConsumer;

            _logger = loggerFactory.CreateLogger<BackOfficeConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var backOfficeProjector =
                new ConnectedProjector<ConsumerAddressContext>(
                    Resolve.WhenEqualToHandlerMessageType(new BackOfficeKafkaProjection().Handlers));

            var commandHandlingProjector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(
                    new CommandHandlingKafkaProjection(_backOfficeContextFactory, _parcels).Handlers));

            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);

            try
            {
                await _kafkaIdompotencyConsumer.ConsumeContinuously(async (message, context) =>
                {
                    await HandleMessage(commandHandlingProjector, commandHandler, message, backOfficeProjector, context);
                }, stoppingToken);
            }
            catch (Exception)
            {
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }

        private async Task HandleMessage(ConnectedProjector<CommandHandler> commandHandlingProjector, CommandHandler commandHandler, object message,
            ConnectedProjector<ConsumerAddressContext> backOfficeProjector, ConsumerAddressContext context)
        {
            _logger.LogInformation("Handling next message");

            await commandHandlingProjector.ProjectAsync(commandHandler, message, CancellationToken.None).ConfigureAwait(false);
            await backOfficeProjector.ProjectAsync(context, message, CancellationToken.None).ConfigureAwait(false);

            //CancellationToken.None to prevent halfway consumption
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
