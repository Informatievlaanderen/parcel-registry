namespace Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using ParcelRegistry.Consumer.Address;

    public interface IKafkaIdompotencyConsumer<out TConsumerContext>
        where TConsumerContext : ConsumerDbContext<TConsumerContext>
    {
        Task ConsumeContinuously(Func<object, TConsumerContext, Task> messageHandler, CancellationToken cancellationToken = default);
        public IdempotentKafkaConsumerOptions ConsumerOptions { get; }
    }

    public sealed class KafkaIdompotencyConsumer<TConsumerContext> : IKafkaIdompotencyConsumer<TConsumerContext>
        where TConsumerContext : ConsumerDbContext<TConsumerContext>
    {
        private readonly IDbContextFactory<TConsumerContext> _dbContextFactory;
        private readonly ILogger _logger;
        private readonly ConsumerConfig _config;
        private readonly JsonSerializer _serializer;

        public IdempotentKafkaConsumerOptions ConsumerOptions { get; }

        public KafkaIdompotencyConsumer(
            IdempotentKafkaConsumerOptions consumerOptions,
            IDbContextFactory<TConsumerContext> dbContextFactory,
            ILoggerFactory loggerFactory)
        {
            ConsumerOptions = consumerOptions;
            _dbContextFactory = dbContextFactory;
            _logger = loggerFactory.CreateLogger<KafkaIdompotencyConsumer<ConsumerAddressContext>>();

            _config = new ConsumerConfig
            {
                BootstrapServers = ConsumerOptions.BootstrapServers,
                GroupId = ConsumerOptions.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }.WithAuthentication(ConsumerOptions);

            _serializer = JsonSerializer.CreateDefault(ConsumerOptions.JsonSerializerSettings);
        }

        public async Task ConsumeContinuously(Func<object, TConsumerContext, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            var consumerBuilder = new ConsumerBuilder<Ignore, string>(_config)
                .SetValueDeserializer(Deserializers.Utf8);
            if (ConsumerOptions.Offset.HasValue)
            {
                consumerBuilder.SetPartitionsAssignedHandler((cons, topicPartitions) =>
                {
                    var partitionOffset = topicPartitions.Select(x => new TopicPartitionOffset(x.Topic, x.Partition, ConsumerOptions.Offset.Value));
                    return partitionOffset;
                });
            }

            using var consumer = consumerBuilder.Build();
            try
            {
                consumer.Subscribe(ConsumerOptions.Topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(3));
                    if (consumeResult == null) //if no message is found, returns null
                    {
                        await Task.Delay(ConsumerOptions.NoMessageFoundDelay, cancellationToken);
                        continue;
                    }

                    var kafkaJsonMessage = _serializer.Deserialize<KafkaJsonMessage>(consumeResult.Message.Value)
                                           ?? throw new ArgumentException("Kafka json message is null.");
                    var messageData = kafkaJsonMessage.Map()
                                      ?? throw new ArgumentException("Kafka message data is null.");

                    var idempotenceKey = consumeResult.Message.Headers.TryGetLastBytes(MessageHeader.IdempotenceKey, out var idempotenceHeaderAsBytes)
                        ? Encoding.UTF8.GetString(idempotenceHeaderAsBytes)
                        : Crypto.Sha512(consumeResult.Message.Value);

                    await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

                    var messageAlreadyProcessed = await dbContext.ProcessedMessages
                        .AsNoTracking()
                        .AnyAsync(x => x.IdempotenceKey == idempotenceKey, cancellationToken)
                        .ConfigureAwait(false);

                    if (messageAlreadyProcessed)
                    {
                        _logger.LogWarning(
                            $"Skipping already processed message at offset '{consumeResult.Offset.Value}' with idempotenceKey '{idempotenceKey}'.");
                        continue;
                    }

                    var processedMessage = new ProcessedMessage(idempotenceKey, DateTimeOffset.Now);

                    try
                    {
                        await dbContext.ProcessedMessages
                            .AddAsync(processedMessage, cancellationToken)
                            .ConfigureAwait(false);

                        await dbContext.SaveChangesAsync(cancellationToken)
                            .ConfigureAwait(false);

                        await messageHandler(messageData, dbContext);

                        consumer.Commit(consumeResult);
                    }
                    catch
                    {
                        dbContext.ProcessedMessages.Remove(processedMessage);
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                        throw;
                    }
                }
            }
            finally
            {
                consumer.Unsubscribe();
            }
        }
    }
}
