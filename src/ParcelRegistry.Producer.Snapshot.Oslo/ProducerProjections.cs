namespace ParcelRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Parcel.Events;

    [ConnectedProjectionName("Kafka producer snapshot oslo")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer, ISnapshotManager snapshotManager)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelWasMigrated>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelAddressWasAttachedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelAddressWasDetachedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelWasImported>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            //When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<ParcelWasRemovedV2>>(async (_, message, ct) =>
            //{
            //    await Produce($"{osloNamespace}/{message.Message.ParcelPersistentLocalId}", "{}", message.Position, ct);
            //});
        }

        private async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, long storePosition, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.JsonContent, storePosition, ct);
            }
        }

        private async Task Produce(string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(objectId),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
