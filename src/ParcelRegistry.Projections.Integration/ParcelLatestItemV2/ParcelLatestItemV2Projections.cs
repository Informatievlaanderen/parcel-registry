namespace ParcelRegistry.Projections.Integration.ParcelLatestItemV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Converters;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("Integratie perceel latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste perceel data voor de integratie database bijhoudt.")]
    public sealed class ParcelLatestItemV2Projections : ConnectedProjection<IntegrationContext>
    {
        public ParcelLatestItemV2Projections(IOptions<IntegrationOptions> options)
        {
            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                await context
                    .ParcelLatestItemsV2
                    .AddAsync(new ParcelLatestItemV2(
                        message.Message.ParcelId,
                        message.Message.CaPaKey,
                        message.Message.ParcelStatus,
                        ParcelStatus.Parse(message.Message.ParcelStatus).ConvertFromParcelStatus(),
                        ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry),
                        $"{options.Value.Namespace}/{message.Message.CaPaKey}",
                        options.Value.Namespace,
                        message.Message.IsRemoved,
                        message.Message.Provenance.Timestamp), ct);

                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context
                        .ParcelLatestItemV2Addresses
                        .AddAsync(new ParcelLatestItemV2Address(
                            message.Message.ParcelId,
                            addressPersistentLocalId,
                            message.Message.CaPaKey), ct);
                }
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                await context
                    .ParcelLatestItemsV2
                    .AddAsync(new ParcelLatestItemV2(
                        message.Message.ParcelId,
                        message.Message.CaPaKey,
                        ParcelStatus.Realized,
                        ParcelStatus.Realized.ConvertFromParcelStatus(),
                        ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry),
                        $"{options.Value.Namespace}/{message.Message.CaPaKey}",
                        options.Value.Namespace,
                        false,
                        message.Message.Provenance.Timestamp), ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        parcel.Geometry = ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry);
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        parcel.Status = ParcelStatus.Realized;
                        parcel.OsloStatus = ParcelStatus.Realized.ConvertFromParcelStatus();
                        parcel.Geometry = ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry);
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        parcel.Status = ParcelStatus.Retired;
                        parcel.OsloStatus = ParcelStatus.Retired.ConvertFromParcelStatus();
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
                await AddParcelAddress(context, message.Message.ParcelId, message.Message.CaPaKey, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId, ct);
                await AddParcelAddress(context, message.Message.ParcelId, message.Message.CaPaKey, message.Message.NewAddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);

                var previousAddress = await context
                    .ParcelLatestItemV2Addresses
                    .FindAsync([message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId], cancellationToken: ct);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    context.ParcelLatestItemV2Addresses.Remove(previousAddress);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress = await context
                    .ParcelLatestItemV2Addresses
                    .FindAsync([message.Message.ParcelId, message.Message.NewAddressPersistentLocalId], cancellationToken: ct);

                if (newAddress is null || context.Entry(newAddress).State == EntityState.Deleted)
                {
                    await context
                        .ParcelLatestItemV2Addresses
                        .AddAsync(new ParcelLatestItemV2Address(
                            message.Message.ParcelId,
                            message.Message.NewAddressPersistentLocalId,
                            message.Message.CaPaKey), ct);
                }
                else
                {
                    newAddress.Count += 1;
                }
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);

                foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                {
                    await RemoveParcelAddress(context, message.Message.ParcelId, addressPersistentLocalId, ct);
                }

                foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                {
                    await AddParcelAddress(context, message.Message.ParcelId, message.Message.CaPaKey, addressPersistentLocalId, ct);
                }
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcel(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersionTimestamp(parcel, message.Message.Provenance.Timestamp);
                    }, ct);
                await RemoveParcelAddress(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });
        }

        private static async Task RemoveParcelAddress(
            IntegrationContext context,
            Guid parcelId,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var latestItemAddress = await context
                .ParcelLatestItemV2Addresses
                .FindAsync(new object?[] { parcelId, addressPersistentLocalId }, cancellationToken: ct);

            if (latestItemAddress is not null)
            {
                context.ParcelLatestItemV2Addresses.Remove(latestItemAddress);
            }
        }

        private static async Task AddParcelAddress(
            IntegrationContext context,
            Guid parcelId,
            string caPaKey,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var newAddress = await context
                .ParcelLatestItemV2Addresses
                .FindAsync([parcelId, addressPersistentLocalId], cancellationToken: ct);

            if (newAddress is null || context.Entry(newAddress).State == EntityState.Deleted)
            {
                await context
                    .ParcelLatestItemV2Addresses
                    .AddAsync(new ParcelLatestItemV2Address(
                        parcelId,
                        addressPersistentLocalId,
                        caPaKey), ct);
            }
        }

        private static void UpdateVersionTimestamp(ParcelLatestItemV2 parcel, Instant versionTimestamp)
            => parcel.VersionTimestamp = versionTimestamp;
    }
}
