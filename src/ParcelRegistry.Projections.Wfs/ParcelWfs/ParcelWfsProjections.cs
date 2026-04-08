namespace ParcelRegistry.Projections.Wfs.ParcelWfs
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("WFS percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het WFS percelenregister voorziet.")]
    public class ParcelWfsProjections : ConnectedProjection<WfsContext>
    {
        public ParcelWfsProjections()
        {
            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var caPaKey = CaPaKey.CreateFrom(message.Message.CaPaKey);

                var item = new ParcelWfsItem(
                    message.Message.ParcelId,
                    caPaKey.CaPaKeyCrabNotation2!,
                    message.Message.CaPaKey,
                    ParcelStatus.Parse(message.Message.ParcelStatus),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                await context.ParcelWfsItems.AddAsync(item, ct);

                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.ParcelWfsAddresses.AddAsync(
                        new ParcelWfsAddressItem(message.Message.ParcelId, addressPersistentLocalId), ct);
                }
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var caPaKey = CaPaKey.CreateFrom(message.Message.CaPaKey);

                var item = new ParcelWfsItem(
                    message.Message.ParcelId,
                    caPaKey.CaPaKeyCrabNotation2!,
                    message.Message.CaPaKey,
                    ParcelStatus.Realized,
                    false,
                    message.Message.Provenance.Timestamp);

                await context.ParcelWfsItems.AddAsync(item, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Realized;
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await context.ParcelWfsAddresses.AddAsync(
                    new ParcelWfsAddressItem(message.Message.ParcelId, message.Message.AddressPersistentLocalId), ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId, ct);

                var existing = await context.ParcelWfsAddresses.FindAsync(
                    [message.Message.ParcelId, message.Message.NewAddressPersistentLocalId], ct);
                if (existing is null)
                {
                    await context.ParcelWfsAddresses.AddAsync(
                        new ParcelWfsAddressItem(message.Message.ParcelId, message.Message.NewAddressPersistentLocalId), ct);
                }
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                var previousAddress = await context.ParcelWfsAddresses.FindAsync(
                    [message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId], ct);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    context.ParcelWfsAddresses.Remove(previousAddress);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress = await context.ParcelWfsAddresses.FindAsync(
                    [message.Message.ParcelId, message.Message.NewAddressPersistentLocalId], ct);

                if (newAddress is null)
                {
                    await context.ParcelWfsAddresses.AddAsync(
                        new ParcelWfsAddressItem(message.Message.ParcelId, message.Message.NewAddressPersistentLocalId), ct);
                }
                else
                {
                    newAddress.Count += 1;
                }
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelWfs(
                    message.Message.ParcelId,
                    entity =>
                    {
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);

                foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                {
                    await RemoveParcelAddress(context, message.Message.ParcelId, addressPersistentLocalId, ct);
                }

                foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                {
                    var existing = await context.ParcelWfsAddresses.FindAsync(
                        [message.Message.ParcelId, addressPersistentLocalId], ct);
                    if (existing is null)
                    {
                        await context.ParcelWfsAddresses.AddAsync(
                            new ParcelWfsAddressItem(message.Message.ParcelId, addressPersistentLocalId), ct);
                    }
                }
            });
        }

        private static async Task RemoveParcelAddress(
            WfsContext context,
            Guid parcelId,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var address = await context.ParcelWfsAddresses.FindAsync([parcelId, addressPersistentLocalId], ct);
            if (address is not null)
            {
                context.ParcelWfsAddresses.Remove(address);
            }
        }

        private static void UpdateVersionTimestamp(ParcelWfsItem item, Instant versionTimestamp)
            => item.VersionTimestamp = versionTimestamp;
    }
}
