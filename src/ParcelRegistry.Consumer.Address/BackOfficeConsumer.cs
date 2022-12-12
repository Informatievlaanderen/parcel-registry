namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Confluent.Kafka;
    using Microsoft.Extensions.Logging;
    using Projections;

    public sealed class BackOfficeConsumer
    {
        private readonly Func<ConsumerAddressContext> _dbContextFactory;
        private readonly KafkaOptions _options;
        private readonly string _topic;
        private readonly string _consumerGroupSuffix;
        private readonly Offset? _offset;
        private readonly ILogger<BackOfficeConsumer> _logger;

        public BackOfficeConsumer(
            Func<ConsumerAddressContext> dbContextFactory,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            string topic,
            string consumerGroupSuffix,
            Offset? offset)
        {
            _dbContextFactory = dbContextFactory;
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
            var consumerAddressContext = _dbContextFactory();

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
                        await projector.ProjectAsync(consumerAddressContext, message, CancellationToken.None);

                        messageCounter++;

                        if (messageCounter % 1000 == 0)
                        {
                            await consumerAddressContext.DisposeAsync();
                            consumerAddressContext = _dbContextFactory();

                            messageCounter = 0;
                        }
                    },
                    noMessageFoundDelay: 300,
                    _offset,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }
    }
}
