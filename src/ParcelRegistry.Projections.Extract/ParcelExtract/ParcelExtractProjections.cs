namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Parcel.Events;
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using Parcel.Events.Crab;

    public class ParcelExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string InUse = "InGebruik";
        private const string Retired = "Gehistoreerd";
        private readonly Encoding _encoding;

        public ParcelExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ParcelWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .ParcelExtract
                    .AddAsync(new ParcelExtractItem
                    {
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.VbrCaPaKey,
                        DbaseRecord = new ParcelDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                            id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.VbrCaPaKey}" },
                            perceelid = { Value = message.Message.VbrCaPaKey }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<ParcelWasRetired>>(async (context, message, ct) =>
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

            When<Envelope<ParcelWasCorrectedToRetired>>(async (context, message, ct) =>
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

            When<Envelope<ParcelWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateStatus(parcel, InUse);
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateStatus(parcel, InUse);
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    SetDelete,
                    ct);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private void SetDelete(ParcelExtractItem parcel)
            => UpdateRecord(parcel, record => record.IsDeleted = true);

        private void UpdateStatus(ParcelExtractItem parcel, string status)
            => UpdateRecord(parcel, record => record.status.Value = status);

        private void UpdateVersie(ParcelExtractItem parcel, Instant version)
            => UpdateRecord(parcel, record => record.versieid.SetValue(version.ToBelgianDateTimeOffset()));

        private void UpdateRecord(ParcelExtractItem parcel, Action<ParcelDbaseRecord> updateFunc)
        {
            var record = new ParcelDbaseRecord();
            record.FromBytes(parcel.DbaseRecord, _encoding);

            updateFunc(record);

            parcel.DbaseRecord = record.ToBytes(_encoding);
        }

        private static void DoNothing() { }
    }
}
