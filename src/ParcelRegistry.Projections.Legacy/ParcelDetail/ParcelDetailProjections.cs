namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using Parcel.Events;
    using System.Linq;
    using System.Threading.Tasks;
    using Parcel.Events.Crab;

    [ConnectedProjectionName("API endpoint detail/lijst percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het percelen detail & lijst voorziet.")]
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

                        if (!entity.Addresses.Any(parcelAddress => parcelAddress.AddressId == message.Message.AddressId && parcelAddress.ParcelId == message.Message.ParcelId))
                        {
                            entity.Addresses.Add(new ParcelDetailAddress
                            {
                                ParcelId = message.Message.ParcelId,
                                AddressId = message.Message.AddressId
                            });
                        }

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
                        if (address is not null)
                        {
                            entity.Addresses.Remove(address);
                        }

                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
        }

        private static void UpdateVersionTimestamp(ParcelDetail parcel, Instant versionTimestamp)
            => parcel.VersionTimestamp = versionTimestamp;

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
