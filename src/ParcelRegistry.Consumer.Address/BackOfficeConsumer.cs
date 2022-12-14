namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Projections;

    public sealed class BackOfficeConsumer : BackgroundService
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly Func<ConsumerAddressContext> _dbContextFactory;
        private readonly Func<BackOfficeContext> _backOfficeContextFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IKafkaIdompotencyConsumer<ConsumerAddressContext> _kafkaIdemIdompotencyConsumer;
        private readonly ILogger<BackOfficeConsumer> _logger;

        public BackOfficeConsumer(
            ILifetimeScope lifetimeScope,
            IHostApplicationLifetime hostApplicationLifetime,
            Func<ConsumerAddressContext> dbContextFactory,
            Func<BackOfficeContext> backOfficeContextFactory,
            ILoggerFactory loggerFactory,
            IKafkaIdompotencyConsumer<ConsumerAddressContext> kafkaIdemIdompotencyConsumer)
        {
            _lifetimeScope = lifetimeScope;
            _hostApplicationLifetime = hostApplicationLifetime;
            _dbContextFactory = dbContextFactory;
            _backOfficeContextFactory = backOfficeContextFactory;
            _loggerFactory = loggerFactory;
            _kafkaIdemIdompotencyConsumer = kafkaIdemIdompotencyConsumer;

            _logger = loggerFactory.CreateLogger<BackOfficeConsumer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ValidateBackOfficeConsumerOffset(stoppingToken);

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

                    await commandHandlingProjector.ProjectAsync(commandHandler, message, stoppingToken)
                        .ConfigureAwait(false);
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

        private async Task ValidateBackOfficeConsumerOffset(CancellationToken cancellationToken)
        {
            if (_kafkaIdemIdompotencyConsumer.ConsumerOptions.Offset is not null)
            {
                await using (var context = _dbContextFactory())
                {
                    if (await context.AddressConsumerItems.AnyAsync(cancellationToken))
                    {
                        throw new InvalidOperationException(
                            "Cannot start consumer from offset, because consumer context already has data. Remove offset or clear data to continue.");
                    }
                }

                _logger.LogInformation($"{nameof(BackOfficeConsumer)} starting {_kafkaIdemIdompotencyConsumer.ConsumerOptions.Topic} from offset {_kafkaIdemIdompotencyConsumer.ConsumerOptions.Offset.Value}.");
            }

            _logger.LogInformation($"{nameof(BackOfficeConsumer)} continuing {_kafkaIdemIdompotencyConsumer.ConsumerOptions.Topic} from last offset.");
        }
    }
}
