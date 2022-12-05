namespace ParcelRegistry.Consumer.Address
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Confluent.Kafka;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class Consumer
    {
        private readonly ConsumerAddressContext _consumerContext;
        private readonly KafkaOptions _options;
        private readonly string _topic;
        private readonly string _consumerGroupSuffix;
        private readonly Offset? _offset;
        private readonly ILogger<Consumer> _logger;

        public Consumer(
            ConsumerAddressContext consumerContext,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            string topic,
            string consumerGroupSuffix,
            Offset? offset)
        {
            _consumerContext = consumerContext;
            _options = options;
            _topic = topic;
            _consumerGroupSuffix = consumerGroupSuffix;
            _offset = offset;

            _logger = loggerFactory.CreateLogger<Consumer>();
        }

        public Task<Result<KafkaJsonMessage>> Start(CancellationToken cancellationToken = default)
        {
            var messageCounter = 0;
            var projector = new ConnectedProjector<ConsumerAddressContext>(Resolve.WhenEqualToHandlerMessageType(new BackOfficeKafkaProjection().Handlers));

            var consumerGroupId = $"{nameof(ParcelRegistry)}.{nameof(Consumer)}.{_topic}{_consumerGroupSuffix}";
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
                        await projector.ProjectAsync(_consumerContext, message, CancellationToken.None);
                        await _consumerContext.SaveChangesAsync(CancellationToken.None);
                        messageCounter++;
                        if (messageCounter % 1000 == 0)
                        {
                            _consumerContext.ChangeTracker.Clear();
                        }
                    },
                    noMessageFoundDelay: 300,
                    _offset,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }
    }
}
