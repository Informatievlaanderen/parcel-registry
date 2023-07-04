namespace ParcelRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Legacy.Events;
    using Legacy.Events.Crab;
    using Parcel.Events;

    [ConnectedProjectionName("Cache markering percelen")]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel percelen de gecachte data nog geüpdated moeten worden.")]
    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml, AcceptType.JsonLd };

        public LastChangedListProjections()
            : base(SupportedAcceptTypes)
        {
            #region Legacy
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

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (_, _, _) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (_, _, _) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (_, _, _) => await DoNothing());

            When<Envelope<ParcelWasMarkedAsMigrated>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
                context.LastChangedList.RemoveRange(records);
            });
            #endregion

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.ParcelId);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.ParcelId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.ParcelId);
            });
        }

        private static void RebuildKeyAndUri(IEnumerable<LastChangedRecord>? attachedRecords, Guid parcelId)
        {
            if (attachedRecords == null)
            {
                return;
            }

            foreach (var record in attachedRecords)
            {
                if (record.CacheKey != null)
                {
                    record.CacheKey = string.Format(record.CacheKey, parcelId.ToString());
                }

                if (record.Uri != null)
                {
                    record.Uri = string.Format(record.Uri, parcelId.ToString());
                }
            }
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

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
