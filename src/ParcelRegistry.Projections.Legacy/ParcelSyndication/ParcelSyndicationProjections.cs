namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Events;
    using ParcelRegistry.Legacy.Events.Crab;

    [ConnectedProjectionName("Feed endpoint percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor de percelen feed voorziet.")]
    public class ParcelSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public ParcelSyndicationProjections()
        {
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
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
