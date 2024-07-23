namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Parcel;
    using Parcel.Events;
    using ParcelRegistry.Legacy.Events;
    using ParcelRegistry.Legacy.Events.Crab;
    using ParcelStatus = ParcelRegistry.Legacy.ParcelStatus;

    [ConnectedProjectionName("Feed endpoint percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor de percelen feed voorziet.")]
    public class ParcelSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public ParcelSyndicationProjections()
        {
            #region Legacy

            When<Envelope<ParcelWasRegistered>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = new ParcelSyndicationItem
                {
                    Position = message.Position,
                    ParcelId = message.Message.ParcelId,
                    CaPaKey = message.Message.VbrCaPaKey,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.Now
                };

                parcelSyndicationItem.ApplyProvenance(message.Message.Provenance);
                parcelSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem, ct);
            });

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Realized,
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Realized,
                    ct);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Retired,
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.Status = ParcelStatus.Retired,
                    ct);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        foreach (var addressId in x.AddressIds)
                        {
                            x.RemoveAddressId(addressId);
                        }
                    },
                    ct);
            });

            When<Envelope<ParcelWasRecovered>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        x.Status = null;
                        foreach (var addressId in x.AddressIds)
                        {
                            x.RemoveAddressId(addressId);
                        }
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.AddAddressId(message.Message.AddressId),
                    ct);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.RemoveAddressId(message.Message.AddressId),
                    ct);
            });

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());

            #endregion

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = new ParcelSyndicationItem
                {
                    Position = message.Position,
                    ParcelId = message.Message.ParcelId,
                    CaPaKey = message.Message.CaPaKey,
                    Status = ParcelStatus.Parse(message.Message.ParcelStatus),
                    ExtendedWkbGeometry = new ExtendedWkbGeometry(message.Message.ExtendedWkbGeometry),
                    AddressPersistentLocalIds = message.Message.AddressPersistentLocalIds,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.Now
                };

                parcelSyndicationItem.ApplyProvenance(message.Message.Provenance);
                parcelSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem, ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = new ParcelSyndicationItem
                {
                    Position = message.Position,
                    ParcelId = message.Message.ParcelId,
                    CaPaKey = message.Message.CaPaKey,
                    Status = ParcelStatus.Realized,
                    ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.Now
                };

                parcelSyndicationItem.ApplyProvenance(message.Message.Provenance);
                parcelSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        x.Status = ParcelStatus.Retired;
                    },
                    ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        x.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        x.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry.ToByteArray();
                        x.Status = ParcelStatus.Realized;
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.AddAddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        x.RemoveAllAddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId);
                        x.AddAddressPersistentLocalId(message.Message.NewAddressPersistentLocalId);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.RemoveAllAddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.RemoveAllAddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.RemoveAllAddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x => x.RemoveAllAddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        x.RemoveAddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId);
                        x.AddAddressPersistentLocalId(message.Message.NewAddressPersistentLocalId);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewParcelSyndicationItem(
                    message.Message.ParcelId,
                    message,
                    x =>
                    {
                        foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                        {
                            x.RemoveAllAddressPersistentLocalId(addressPersistentLocalId);
                        }

                        foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                        {
                            x.AddAddressPersistentLocalId(addressPersistentLocalId);
                        }
                    },
                    ct);
            });
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
