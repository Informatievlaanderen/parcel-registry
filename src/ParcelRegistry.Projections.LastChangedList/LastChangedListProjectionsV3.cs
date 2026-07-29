namespace ParcelRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Parcel.Events;

    [ConnectedProjectionName(ProjectionName)]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel percelen de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedListProjectionsV3 : LastChangedListConnectedProjection
    {
        public const string ProjectionName = "Cache markering percelen (v3)";
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.JsonLd };

        public LastChangedListProjectionsV3(ICacheValidator cacheValidator)
            : base(SupportedAcceptTypes, cacheValidator)
        {
            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.CaPaKey);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.CaPaKey);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(GetIdentifier(message.Message.ParcelId.ToString()), message.Position, context, ct);
            });
        }

        private static string GetIdentifier(string parcelId) => $"v3.{parcelId}";

        private static void RebuildKeyAndUri(IEnumerable<LastChangedRecord>? attachedRecords, string caPaKey)
        {
            if (attachedRecords == null)
            {
                return;
            }

            foreach (var record in attachedRecords)
            {
                if (record.CacheKey != null)
                {
                    record.CacheKey = string.Format(record.CacheKey, caPaKey);
                }

                if (record.Uri != null)
                {
                    record.Uri = string.Format(record.Uri, caPaKey);
                }
            }
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.JsonLd => $"oslo-v3/parcel:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.JsonLd => $"/v3/percelen/{{0}}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }
    }
}
