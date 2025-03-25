namespace ParcelRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Newtonsoft.Json;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("Kafka producer ldes")]
    [ConnectedProjectionDescription("Projectie die ldes berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;
        private readonly string _osloNamespace;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ProducerProjections(
            IProducer producer,
            string osloNamespace,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _producer = producer;
            _osloNamespace = osloNamespace;
            _jsonSerializerSettings = jsonSerializerSettings;

            When<Envelope<ParcelOsloSnapshotsWereRequested>>(async (context, message, ct) =>
            {
                foreach (var parcelId in message.Message.ParcelIdsWithCapaKey.Keys)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    await Produce(context, parcelId, message.Position, ct);
                }
            });

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                await context.Parcels.AddAsync(
                    new ParcelDetail(
                        message.Message.ParcelId,
                        message.Message.CaPaKey,
                        ParcelStatus.Parse(message.Message.ParcelStatus),
                        message.Message.AddressPersistentLocalIds.Select(x => new ParcelDetailAddress(message.Message.ParcelId, x)),
                        message.Message.IsRemoved,
                        message.Message.Provenance.Timestamp)
                    , ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        AddParcelAddress(context, entity, message.Message.AddressPersistentLocalId);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        RemoveParcelAddress(context, entity, message.Message.PreviousAddressPersistentLocalId);
                        AddParcelAddress(context, entity, message.Message.NewAddressPersistentLocalId);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        RemoveParcelAddress(context, entity, message.Message.AddressPersistentLocalId);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        RemoveParcelAddress(context, entity, message.Message.AddressPersistentLocalId);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        RemoveParcelAddress(context, entity, message.Message.AddressPersistentLocalId);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        RemoveParcelAddress(context, entity, message.Message.AddressPersistentLocalId);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var previousAddress = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);

                        if (previousAddress is not null && previousAddress.Count == 1)
                        {
                            entity.Addresses.Remove(previousAddress);
                        }
                        else if (previousAddress is not null)
                        {
                            previousAddress.Count -= 1;
                        }

                        var newAddress = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);

                        if (newAddress is null)
                        {
                            entity.Addresses.Add(new ParcelDetailAddress(message.Message.ParcelId, message.Message.NewAddressPersistentLocalId));
                        }
                        else
                        {
                            newAddress.Count += 1;
                        }

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                        {
                            RemoveParcelAddress(context, entity, addressPersistentLocalId);
                        }

                        foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                        {
                            AddParcelAddress(context, entity, addressPersistentLocalId);
                        }

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var item = new ParcelDetail(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    ParcelStatus.Realized,
                    new List<ParcelDetailAddress>(),
                    false,
                    message.Message.Provenance.Timestamp);

                await context
                    .Parcels
                    .AddAsync(item, ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Realized;

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.ParcelId, message.Position, ct);
            });
        }

        private static void AddParcelAddress(
            ProducerContext context,
            ParcelDetail entity,
            int addressPersistentLocalId)
        {
            context.Entry(entity).Collection(x => x.Addresses).Load();

            if (!entity.Addresses.Any(parcelAddress =>
                    parcelAddress.AddressPersistentLocalId == addressPersistentLocalId
                    && parcelAddress.ParcelId == entity.ParcelId))
            {
                entity.Addresses.Add(new ParcelDetailAddress(entity.ParcelId, addressPersistentLocalId));
            }
        }

        private static void RemoveParcelAddress(
            ProducerContext context,
            ParcelDetail entity,
            int addressPersistentLocalId)
        {
            context.Entry(entity).Collection(x => x.Addresses).Load();

            var addressToRemove = entity.Addresses.SingleOrDefault(parcelAddress =>
                parcelAddress.AddressPersistentLocalId == addressPersistentLocalId
                && parcelAddress.ParcelId == entity.ParcelId);
            if (addressToRemove is not null)
            {
                entity.Addresses.Remove(addressToRemove);
            }
        }

        private static void UpdateVersionTimestamp(ParcelDetail item, Instant versionTimestamp)
            => item.VersionTimestamp = versionTimestamp;

        private async Task Produce(
            ProducerContext context,
            Guid parcelId,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var parcel = await context.Parcels.FindAsync(parcelId, cancellationToken: cancellationToken)
                             ?? throw new ProjectionItemNotFoundException<ProducerProjections>(parcelId.ToString());

            var parcelLdes = new ParcelLdes(parcel, _osloNamespace);

            await Produce(
                $"{_osloNamespace}/{parcel.CaPaKey}",
                parcel.CaPaKey,
                JsonConvert.SerializeObject(parcelLdes, _jsonSerializerSettings),
                storePosition,
                cancellationToken);
        }

        private async Task Produce(
            string puri,
            string objectId,
            string jsonContent,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
