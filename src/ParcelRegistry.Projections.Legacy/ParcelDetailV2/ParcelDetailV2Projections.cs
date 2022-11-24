namespace ParcelRegistry.Projections.Legacy.ParcelDetailV2
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("API endpoint detail/lijst percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het percelen detail & lijst voorziet.")]
    public class ParcelDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public ParcelDetailV2Projections()
        {
            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var item = new ParcelDetailV2(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    ParcelStatus.Parse(message.Message.ParcelStatus),
                    message.Message.AddressPersistentLocalIds.Select(x => new ParcelDetailAddressV2(message.Message.ParcelId, x)),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                UpdateHash(item, message);

                await context
                    .ParcelDetailV2
                    .AddAsync(item, ct);
            });
        }

        private static void UpdateHash<T>(ParcelDetailV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static void UpdateVersionTimestamp(ParcelDetailV2 item, Instant versionTimestamp)
            => item.VersionTimestamp = versionTimestamp;
    }
}
