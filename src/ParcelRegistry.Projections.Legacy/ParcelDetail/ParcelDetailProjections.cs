namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Parcel.Events;

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
                            OsloId = message.Message.VbrCaPaKey,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            Complete = true,
                            Removed = false
                        });
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Removed = true;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
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
                // TODO: How is this going to behave in catch-up mode?
                var item = await context.ParcelDetail
                    .Include(x => x.Addresses)
                    .FirstOrDefaultAsync(x => x.ParcelId == message.Message.ParcelId);

                // TODO: look up oslo id of address
                item?.Addresses.Add(new ParcelDetailAddress
                {
                    ParcelId = message.Message.ParcelId,
                    AddressId = message.Message.AddressId,
                });

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                // TODO: How is this going to behave in catch-up mode?
                var item = await context.ParcelDetail
                    .Include(x => x.Addresses)
                    .FirstOrDefaultAsync(x => x.ParcelId == message.Message.ParcelId);

                if (item != null)
                {
                    var address = item.Addresses.SingleOrDefault(x => x.AddressId == message.Message.AddressId);
                    item.Addresses.Remove(address);
                }

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });
        }

        private static void UpdateVersionTimestamp(ParcelDetail parcel, Instant versionTimestamp)
            => parcel.VersionTimestamp = versionTimestamp;
    }
}
