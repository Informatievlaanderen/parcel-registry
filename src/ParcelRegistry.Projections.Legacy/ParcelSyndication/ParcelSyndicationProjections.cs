namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;
    using Parcel.Events;

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
                    RecordCreatedAt = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    LastChangedOn = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    ChangeType = message.EventName,
                    IsComplete = true,
                };

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = ParcelStatus.Realized);

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = ParcelStatus.Realized);

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = ParcelStatus.Retired);

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = ParcelStatus.Retired);

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => { });

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.AddAddressId(message.Message.AddressId));

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                var parcelSyndicationItem = await context.LatestPosition(message.Message.ParcelId, ct);

                parcelSyndicationItem = parcelSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.RemoveAddressId(message.Message.AddressId));

                ApplyProvenance(parcelSyndicationItem, message.Message.Provenance);

                await context
                    .ParcelSyndication
                    .AddAsync(parcelSyndicationItem);
            });
        }

        private static void ApplyProvenance(ParcelSyndicationItem item, ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Plan = provenance.Plan;
        }
    }
}
