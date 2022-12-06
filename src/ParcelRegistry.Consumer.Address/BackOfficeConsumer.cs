namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Confluent.Kafka;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class BackOfficeConsumer : IDisposable
    {
        private readonly ConsumerAddressContext _consumerAddressContext;
        private readonly KafkaOptions _options;
        private readonly string _topic;
        private readonly string _consumerGroupSuffix;
        private readonly Offset? _offset;
        private readonly ILogger<BackOfficeConsumer> _logger;

        public BackOfficeConsumer(
            ILifetimeScope lifetimeScope,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            string topic,
            string consumerGroupSuffix,
            Offset? offset)
        {
            _consumerAddressContext = lifetimeScope.Resolve<ConsumerAddressContext>();
            _options = options;
            _topic = topic;
            _consumerGroupSuffix = consumerGroupSuffix;
            _offset = offset;

            _logger = loggerFactory.CreateLogger<BackOfficeConsumer>();
        }

        public Task<Result<KafkaJsonMessage>> Start(CancellationToken cancellationToken = default)
        {
            var messageCounter = 0;

            var projector =
                new ConnectedProjector<ConsumerAddressContext>(
                    Resolve.WhenEqualToHandlerMessageType(new BackOfficeKafkaProjection().Handlers));

            var consumerGroupId =
                $"{nameof(ParcelRegistry)}.{nameof(BackOfficeConsumer)}.{_topic}{_consumerGroupSuffix}";
            return KafkaConsumer.Consume(
                new KafkaConsumerOptions(
                    _options.BootstrapServers,
                    _options.SaslUserName,
                    _options.SaslPassword,
                    consumerGroupId,
                    _topic,
                    async message =>
                    {
                        _logger.LogInformation("Handling next message");
                        //CancellationToken.None to prevent halfway consumption
                        await projector.ProjectAsync(_consumerAddressContext, message, CancellationToken.None);
                        await _consumerAddressContext.SaveChangesAsync(CancellationToken.None);
                        messageCounter++;
                        if (messageCounter % 1000 == 0)
                        {
                            _consumerAddressContext.ChangeTracker.Clear();
                        }
                    },
                    noMessageFoundDelay: 300,
                    _offset,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }

        public void Dispose()
        {
            _consumerAddressContext.Dispose();
        }
    }
}
