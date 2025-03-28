namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("Extract percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het percelen extract voorziet.")]
    public class ParcelExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";
        private readonly Encoding _encoding;

        public ParcelExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                await context
                    .ParcelExtract
                    .AddAsync(new ParcelExtractItem
                    {
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.CaPaKey,
                        DbaseRecord = new ParcelDbaseRecord
                        {
                            id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.CaPaKey}" },
                            perceelid = { Value = message.Message.CaPaKey },
                            status = { Value = MapStatus(ParcelStatus.Parse(message.Message.ParcelStatus)) },
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                await context
                    .ParcelExtract
                    .AddAsync(new ParcelExtractItem
                    {
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.CaPaKey,
                        DbaseRecord = new ParcelDbaseRecord
                        {
                            id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.CaPaKey}" },
                            perceelid = { Value = message.Message.CaPaKey },
                            status = { Value = Realized },
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateStatus(parcel, Retired);
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateStatus(parcel, Realized);
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });
        }

        private void SetDelete(ParcelExtractItem parcel)
            => UpdateRecord(parcel, record => record.IsDeleted = true);

        private static readonly IDictionary<ParcelStatus, string> StatusMapping = new Dictionary<ParcelStatus, string>
        {
            { ParcelStatus.Realized, Realized },
            { ParcelStatus.Retired, Retired }
        };

        private string MapStatus(ParcelStatus parcelStatus)
        {
            return StatusMapping.ContainsKey(parcelStatus)
                ? StatusMapping[parcelStatus]
                : throw new ArgumentOutOfRangeException(nameof(parcelStatus), parcelStatus, null);
        }

        private void UpdateStatus(ParcelExtractItem parcel, string status)
            => UpdateRecord(parcel, record => record.status.Value = status);

        private void UpdateVersie(ParcelExtractItem parcel, Instant version)
            => UpdateRecord(parcel, record => record.versieid.SetValue(version.ToBelgianDateTimeOffset()));

        private void UpdateRecord(ParcelExtractItem parcel, Action<ParcelDbaseRecord> updateFunc)
        {
            if (parcel.DbaseRecord is not null)
            {
                var record = new ParcelDbaseRecord();
                record.FromBytes(parcel.DbaseRecord, _encoding);

                updateFunc(record);

                parcel.DbaseRecord = record.ToBytes(_encoding);
            }
        }
    }
}
