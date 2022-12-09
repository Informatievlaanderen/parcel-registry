namespace ParcelRegistry.Consumer.Address
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Extensions.Logging;
    using Projections;

    public sealed class CommandHandlingConsumer
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaOptions _options;
        private readonly string _topic;
        private readonly string _consumerGroupSuffix;
        private readonly ILogger<CommandHandlingConsumer> _logger;

        public CommandHandlingConsumer(
            ILifetimeScope lifetimeScope,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            string topic,
            string consumerGroupSuffix)
        {
            _lifetimeScope = lifetimeScope;
            _loggerFactory = loggerFactory;
            _options = options;
            _topic = topic;
            _consumerGroupSuffix = consumerGroupSuffix;

            _logger = loggerFactory.CreateLogger<CommandHandlingConsumer>();
        }

        public Task<Result<KafkaJsonMessage>> Start(CancellationToken cancellationToken = default)
        {
            using var backofficeContext = _lifetimeScope.Resolve<BackOfficeContext>();

            var projector = new ConnectedProjector<CommandHandler>(
                Resolve.WhenEqualToHandlerMessageType(
                    new CommandHandlingKafkaProjection(backofficeContext).Handlers));

            var commandHandler = new CommandHandler(_lifetimeScope, _loggerFactory);

            var consumerGroupId = $"{nameof(ParcelRegistry)}.{nameof(CommandHandlingConsumer)}.{_topic}{_consumerGroupSuffix}";
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
                        await projector.ProjectAsync(commandHandler, message, CancellationToken.None);
                    },
                    noMessageFoundDelay: 300,
                    null,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }
    }
}
