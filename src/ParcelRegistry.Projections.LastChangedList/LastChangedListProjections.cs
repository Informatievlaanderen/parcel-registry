namespace ParcelRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Parcel.Events;
    using Parcel.Events.Crab;

    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        protected override string CacheKeyFormat => "legacy/parcel:{{0}}.{1}";
        protected override string UriFormat => "/v1/percelen/{{0}}";

        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, /*AcceptType.JsonLd,*/ AcceptType.Xml };

        public LastChangedListProjections()
            : base(SupportedAcceptTypes)
        {
            When<Envelope<ParcelWasRegistered>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);

                foreach (var lastChangedRecord in records)
                {
                    lastChangedRecord.CacheKey = string.Format(lastChangedRecord.CacheKey, message.Message.VbrCaPaKey);
                    lastChangedRecord.Uri = string.Format(lastChangedRecord.Uri, message.Message.VbrCaPaKey);
                }
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void DoNothing() { }
    }
}
