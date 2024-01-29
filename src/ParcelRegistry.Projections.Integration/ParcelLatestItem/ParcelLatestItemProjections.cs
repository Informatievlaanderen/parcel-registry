namespace ParcelRegistry.Projections.Integration.ParcelLatestItem
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Converters;
    using Infrastructure;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("Integratie perceel latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste perceel data voor de integratie database bijhoudt.")]
    public sealed class ParcelLatestItemProjections : ConnectedProjection<IntegrationContext>
    {
        public ParcelLatestItemProjections(IOptions<IntegrationOptions> options)
        {
            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                await context
                    .ParcelLatestItems
                    .AddAsync(new ParcelLatestItem(
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
                        .ParcelLatestItemAddresses
                        .AddAsync(new ParcelLatestItemAddress(
                            message.Message.ParcelId,
                            addressPersistentLocalId,
                            message.Message.CaPaKey), ct);
                }
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                await context
                    .ParcelLatestItems
                    .AddAsync(new ParcelLatestItem(
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
                await context
                    .ParcelLatestItemAddresses
                    .AddAsync(new ParcelLatestItemAddress(
                        message.Message.ParcelId,
                        message.Message.AddressPersistentLocalId,
                        message.Message.CaPaKey), ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var latestItemAddress = await context
                    .ParcelLatestItemAddresses
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId }, cancellationToken: ct);

                context.ParcelLatestItemAddresses.Remove(latestItemAddress);

                await context
                    .ParcelLatestItemAddresses
                    .AddAsync(new ParcelLatestItemAddress(
                        message.Message.ParcelId,
                        message.Message.NewAddressPersistentLocalId,
                        message.Message.CaPaKey), ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var latestItemAddress = await context
                    .ParcelLatestItemAddresses
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.AddressPersistentLocalId }, cancellationToken: ct);

                context.ParcelLatestItemAddresses.Remove(latestItemAddress);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var latestItemAddress = await context
                    .ParcelLatestItemAddresses
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.AddressPersistentLocalId }, cancellationToken: ct);

                context.ParcelLatestItemAddresses.Remove(latestItemAddress);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var latestItemAddress = await context
                    .ParcelLatestItemAddresses
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.AddressPersistentLocalId }, cancellationToken: ct);

                context.ParcelLatestItemAddresses.Remove(latestItemAddress);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var latestItemAddress = await context
                    .ParcelLatestItemAddresses
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.AddressPersistentLocalId }, cancellationToken: ct);

                context.ParcelLatestItemAddresses.Remove(latestItemAddress);
            });
        }

        private static void UpdateVersionTimestamp(ParcelLatestItem parcel, Instant versionTimestamp)
            => parcel.VersionTimestamp = versionTimestamp;
    }
}
