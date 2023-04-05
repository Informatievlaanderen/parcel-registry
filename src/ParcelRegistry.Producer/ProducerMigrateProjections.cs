namespace ParcelRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;

    [ConnectedProjectionName("Kafka producer start from migrate")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt startende vanaf migratie.")]
    public class ProducerMigrateProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "MigrationTopic";

        private readonly IProducer _producer;

        public ProducerMigrateProjections(IProducer producer)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelAddressWasAttachedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelAddressWasDetachedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelWasMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(Guid persistentLocalId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(persistentLocalId.ToString()),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
