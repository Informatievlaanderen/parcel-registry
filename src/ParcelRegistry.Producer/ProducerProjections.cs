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
    using ParcelDomain = Parcel.Events;
    using Legacy = Legacy.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer)
        {
            _producer = producer;

            #region Legacy Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelAddressWasAttached>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelAddressWasDetached>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasCorrectedToRealized>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasCorrectedToRetired>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasMarkedAsMigrated>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasRealized>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasRemoved>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasRetired>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasRecovered>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.ParcelWasRegistered>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });
            #endregion

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelAddressWasAttachedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelAddressWasDetachedV2>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelDomain.ParcelWasMigrated>>(async (_, message, ct) =>
            {
               await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelWasImported>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Parcel.Events.ParcelWasRetiredV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.ParcelId, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(Guid parcelId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(parcelId.ToString("D")),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
