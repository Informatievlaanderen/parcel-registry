namespace ParcelRegistry.Projections.LastChangedList
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Parcel.Events;
    using Parcel.Events.Crab;

    [ConnectedProjectionName("Cache markering percelen")]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel percelen de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml, AcceptType.JsonLd };

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

            When<Envelope<ParcelWasRecovered>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.Json => $"legacy/parcel:{{0}}.{shortenedAcceptType}",
                AcceptType.Xml => $"legacy/parcel:{{0}}.{shortenedAcceptType}",
                AcceptType.JsonLd => $"oslo/parcel:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => $"/v1/percelen/{{0}}",
                AcceptType.Xml => $"/v1/percelen/{{0}}",
                AcceptType.JsonLd => $"/v2/percelen/{{0}}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }
        private static void DoNothing() { }
    }
}
