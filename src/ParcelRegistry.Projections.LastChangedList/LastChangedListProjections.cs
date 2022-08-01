namespace ParcelRegistry.Projections.LastChangedList
{
    using System;
    using System.Threading.Tasks;
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
                    if (lastChangedRecord.CacheKey != null)
                    {
                        lastChangedRecord.CacheKey = string.Format(lastChangedRecord.CacheKey, message.Message.VbrCaPaKey);
                    }

                    if (lastChangedRecord.Uri != null)
                    {
                        lastChangedRecord.Uri = string.Format(lastChangedRecord.Uri, message.Message.VbrCaPaKey);
                    }
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

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.Json => $"legacy/parcel:{{{identifier}}}.{shortenedAcceptType}",
                AcceptType.Xml => $"legacy/parcel:{{{identifier}}}.{shortenedAcceptType}",
                AcceptType.JsonLd => $"oslo/parcel:{{{identifier}}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => $"/v1/percelen/{{{identifier}}}",
                AcceptType.Xml => $"/v1/percelen/{{{identifier}}}",
                AcceptType.JsonLd => $"/v2/percelen/{{{identifier}}}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
