namespace ParcelRegistry.Projections.Integration.ParcelVersion
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Converters;
    using Infrastructure;
    using Legacy.Events;
    using Microsoft.Extensions.Options;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("Integratie perceel versie")]
    [ConnectedProjectionDescription("Projectie die de perceel versie data voor de integratie database bijhoudt.")]
    public class ParcelVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public ParcelVersionProjections(
            IAddressRepository addressRepository,
            IOptions<IntegrationOptions> options)
        {
            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                await context
                    .ParcelVersions
                    .AddAsync(new ParcelVersion
                    {
                        Position = message.Position,
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.CaPaKey,
                        Status = message.Message.ParcelStatus,
                        OsloStatus = ParcelStatus.Parse(message.Message.ParcelStatus).ConvertFromParcelStatus(),
                        Geometry = ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry),
                        Puri = $"{options.Value.Namespace}/{message.Message.CaPaKey}",
                        Namespace = options.Value.Namespace,
                        IsRemoved = message.Message.IsRemoved,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                        Type = message.EventName
                    }, ct);

                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context
                        .ParcelVersionAddresses
                        .AddAsync(new ParcelVersionAddress(
                            message.Position,
                            message.Message.ParcelId,
                            addressPersistentLocalId,
                            message.Message.CaPaKey,
                            message.Message.Provenance.Timestamp), ct);
                }
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                await context
                    .ParcelVersions
                    .AddAsync(new ParcelVersion
                    {
                        Position = message.Position,
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.CaPaKey,
                        Status = ParcelStatus.Realized,
                        OsloStatus = ParcelStatus.Realized.ConvertFromParcelStatus(),
                        Geometry = ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry),
                        Puri = $"{options.Value.Namespace}/{message.Message.CaPaKey}",
                        Namespace = options.Value.Namespace,
                        IsRemoved = false,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                        Type = message.EventName
                    }, ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    parcel => { parcel.Geometry = ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry); }, ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    parcel =>
                    {
                        parcel.Status = ParcelStatus.Realized;
                        parcel.OsloStatus = ParcelStatus.Realized.ConvertFromParcelStatus();
                        parcel.Geometry = ParcelMapper.MapExtendedWkbGeometryToGeometry(message.Message.ExtendedWkbGeometry);
                    }, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    parcel =>
                    {
                        parcel.Status = ParcelStatus.Retired;
                        parcel.OsloStatus = ParcelStatus.Retired.ConvertFromParcelStatus();
                    }, ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                await context
                    .ParcelVersionAddresses
                    .AddAsync(new ParcelVersionAddress(
                        message.Position,
                        message.Message.ParcelId,
                        message.Message.AddressPersistentLocalId,
                        message.Message.CaPaKey,
                        message.Message.Provenance.Timestamp), ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                var versionAddress = await context
                    .ParcelVersionAddresses
                    .FindAsync(new object?[] { message.Position, message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId },
                        cancellationToken: ct);

                context.ParcelVersionAddresses.Remove(versionAddress);

                await context
                    .ParcelVersionAddresses
                    .AddAsync(new ParcelVersionAddress(
                        message.Position,
                        message.Message.ParcelId,
                        message.Message.NewAddressPersistentLocalId,
                        message.Message.CaPaKey,
                        message.Message.Provenance.Timestamp), ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                var versionAddress = await context
                    .ParcelVersionAddresses
                    .FindAsync(new object?[] { message.Position, message.Message.ParcelId, message.Message.AddressPersistentLocalId },
                        cancellationToken: ct);

                context.ParcelVersionAddresses.Remove(versionAddress);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                var versionAddress = await context
                    .ParcelVersionAddresses
                    .FindAsync(new object?[] { message.Position, message.Message.ParcelId, message.Message.AddressPersistentLocalId },
                        cancellationToken: ct);

                context.ParcelVersionAddresses.Remove(versionAddress);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                var versionAddress = await context
                    .ParcelVersionAddresses
                    .FindAsync(new object?[] { message.Position, message.Message.ParcelId, message.Message.AddressPersistentLocalId },
                        cancellationToken: ct);

                context.ParcelVersionAddresses.Remove(versionAddress);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                var versionAddress = await context
                    .ParcelVersionAddresses
                    .FindAsync(new object?[] { message.Position, message.Message.ParcelId, message.Message.AddressPersistentLocalId },
                        cancellationToken: ct);

                context.ParcelVersionAddresses.Remove(versionAddress);
            });

            #region Legacy

            When<Envelope<ParcelWasRegistered>>(async (context, message, ct) =>
            {
                var caPaKey = CaPaKey.CreateFrom(message.Message.VbrCaPaKey);
                await context
                    .ParcelVersions
                    .AddAsync(
                        new ParcelVersion()
                        {
                            Position = message.Position,
                            ParcelId = message.Message.ParcelId,
                            CaPaKey = caPaKey,
                            VersionTimestamp = message.Message.Provenance.Timestamp,
                            CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                            Puri = $"{options.Value.Namespace}/{caPaKey}",
                            Namespace = options.Value.Namespace,
                            IsRemoved = false,
                            Type = message.EventName
                        });
            });

            When<Envelope<ParcelWasRecovered>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    entity =>
                    {
                        entity.IsRemoved = false;
                        entity.Status = null;
                        entity.OsloStatus = null;
                    },
                    ct,
                    cloneAddresses: false);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    entity => { entity.IsRemoved = true; },
                    ct,
                    cloneAddresses: false);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;
                        entity.OsloStatus = ParcelStatus.Retired.ConvertFromParcelStatus();
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;
                        entity.OsloStatus = ParcelStatus.Retired.ConvertFromParcelStatus();
                    },
                    ct);
            });

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Realized;
                        entity.OsloStatus = ParcelStatus.Realized.ConvertFromParcelStatus();
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Realized;
                        entity.OsloStatus = ParcelStatus.Realized.ConvertFromParcelStatus();
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) =>
            {
                var addressPersistentLocalId =
                    await addressRepository.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    return;
                }

                var newParcelVersion = await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { },
                    ct);

                await context
                    .ParcelVersionAddresses
                    .AddAsync(new ParcelVersionAddress(
                        message.Position,
                        message.Message.ParcelId,
                        addressPersistentLocalId.Value,
                        newParcelVersion.CaPaKey,
                        message.Message.Provenance.Timestamp), ct);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                var addressPersistentLocalId =
                    await addressRepository.GetAddressPersistentLocalId(message.Message.AddressId);

                if (addressPersistentLocalId is null)
                {
                    return;
                }

                await context.CreateNewParcelVersion(
                    message.Message.ParcelId,
                    message,
                    _ => { }, ct);

                var versionAddress = await context
                    .ParcelVersionAddresses
                    .FindAsync(
                        new object?[] { message.Position, message.Message.ParcelId, addressPersistentLocalId.Value },
                        cancellationToken: ct);

                context.ParcelVersionAddresses.Remove(versionAddress);
            });
        }

        #endregion
    }
}
