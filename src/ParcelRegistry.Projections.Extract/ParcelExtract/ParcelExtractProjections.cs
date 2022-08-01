namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Parcel.Events;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using Parcel.Events.Crab;

    [ConnectedProjectionName("Extract percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het percelen extract voorziet.")]
    public class ParcelExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string InUse = "Gerealiseerd";
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

            When<Envelope<ParcelWasRecovered>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelExtract(
                    message.Message.ParcelId,
                    parcel =>
                    {
                        UpdateRecord(parcel, p =>
                        {
                            p.IsDeleted = false;
                            p.status.Value = string.Empty;
                        });
                        UpdateVersie(parcel, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasAttached>>(async (context, message, ct) => await DoNothing());
            When<Envelope<ParcelAddressWasDetached>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
        }

        private void SetDelete(ParcelExtractItem parcel)
            => UpdateRecord(parcel, record => record.IsDeleted = true);

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

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
