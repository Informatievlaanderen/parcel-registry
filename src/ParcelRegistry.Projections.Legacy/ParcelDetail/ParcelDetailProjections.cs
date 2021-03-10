namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using Parcel.Events;
    using System.Linq;
    using Parcel.Events.Crab;

    [ConnectedProjectionName("Legacy - ParcelDetail")]
    [ConnectedProjectionDescription("Perceel detail data.")]
    public class ParcelDetailProjections : ConnectedProjection<LegacyContext>
    {
        public ParcelDetailProjections()
        {
            When<Envelope<ParcelWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .ParcelDetail
                    .AddAsync(
                        new ParcelDetail
                        {
                            ParcelId = message.Message.ParcelId,
                            PersistentLocalId = message.Message.VbrCaPaKey,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            Removed = false
                        });
            });

            When<Envelope<ParcelWasRecovered>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Removed = false;
                        entity.Status = null;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);

                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        entity.Addresses.Clear();
                    },
                    ct);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Removed = true;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);

                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        entity.Addresses.Clear();
                    },
                    ct);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Realized;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Realized;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        // TODO: look up persistent local id of address
                        entity.Addresses.Add(new ParcelDetailAddress
                        {
                            ParcelId = message.Message.ParcelId,
                            AddressId = message.Message.AddressId,
                        });
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var address = entity.Addresses.SingleOrDefault(x => x.AddressId == message.Message.AddressId);
                        entity.Addresses.Remove(address);

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void UpdateVersionTimestamp(ParcelDetail parcel, Instant versionTimestamp)
            => parcel.VersionTimestamp = versionTimestamp;

        private static void DoNothing() { }
    }
}
