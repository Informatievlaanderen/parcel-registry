namespace ParcelRegistry.Projections.Extract.ParcelLinkExtract
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using ParcelRegistry.Parcel.Events;

    [ConnectedProjectionName("Extract perceelkoppelingen met adres")]
    [ConnectedProjectionDescription("Projectie die de perceel koppelingen data voor het adreskoppelingen extract voorziet.")]
    public sealed class ParcelLinkExtractProjections : ConnectedProjection<ExtractContext>
    {
        public const string ParcelObjectType = "Perceel";

        private readonly Encoding _encoding;

        public ParcelLinkExtractProjections(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                {
                    return;
                }

                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context
                        .ParcelLinkExtract
                        .AddAsync(new ParcelLinkExtractItem
                        {
                            ParcelId = message.Message.ParcelId,
                            CaPaKey = message.Message.CaPaKey,
                            AddressPersistentLocalId = addressPersistentLocalId,
                            Count = 1,
                            DbaseRecord = new ParcelLinkDbaseRecord
                            {
                                objecttype = { Value = ParcelObjectType },
                                adresobjid = { Value = message.Message.CaPaKey },
                                adresid = { Value = addressPersistentLocalId }
                            }.ToBytes(_encoding)
                        }, ct);
                }
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context
                    .ParcelLinkExtract
                    .AddAsync(new ParcelLinkExtractItem
                    {
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.CaPaKey,
                        AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                        Count = 1,
                        DbaseRecord = new ParcelLinkDbaseRecord
                        {
                            objecttype = { Value = ParcelObjectType },
                            adresobjid = { Value = message.Message.CaPaKey },
                            adresid = { Value = message.Message.AddressPersistentLocalId }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await RemoveParcelLink(context, message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId, ct);
                await AddParcelLink(context, message.Message.ParcelId, message.Message.CaPaKey, message.Message.NewAddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await RemoveParcelLink(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await RemoveParcelLink(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await RemoveParcelLink(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await RemoveParcelLink(context, message.Message.ParcelId, message.Message.AddressPersistentLocalId, ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var previousAddress = await context
                    .ParcelLinkExtract
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId }, ct);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    context.ParcelLinkExtract.Remove(previousAddress);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress = await context
                    .ParcelLinkExtract
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.NewAddressPersistentLocalId }, ct);

                if (newAddress is null || context.Entry(newAddress).State == EntityState.Deleted)
                {
                    await context
                        .ParcelLinkExtract
                        .AddAsync(new ParcelLinkExtractItem
                        {
                            ParcelId = message.Message.ParcelId,
                            CaPaKey = message.Message.CaPaKey,
                            AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId,
                            Count = 1,
                            DbaseRecord = new ParcelLinkDbaseRecord
                            {
                                objecttype = { Value = ParcelObjectType },
                                adresobjid = { Value = message.Message.CaPaKey },
                                adresid = { Value = message.Message.NewAddressPersistentLocalId }
                            }.ToBytes(_encoding)
                        }, ct);
                }
                else
                {
                    newAddress.Count += 1;
                }
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                {
                    await RemoveParcelLink(context, message.Message.ParcelId, addressPersistentLocalId, ct);
                }

                foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                {
                    await AddParcelLink(context, message.Message.ParcelId, message.Message.CaPaKey, addressPersistentLocalId, ct);
                }
            });

            When<Envelope<ParcelGeometryWasChanged>>(DoNothing);
            When<Envelope<ParcelWasRetiredV2>>(DoNothing);
            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(DoNothing);
            When<Envelope<ParcelWasImported>>(DoNothing);
        }

        private static async Task RemoveParcelLink(
            ExtractContext context,
            Guid parcelId,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var linkExtractItem = await context
                .ParcelLinkExtract
                .FindAsync(new object?[] { parcelId, addressPersistentLocalId }, ct);

            if (linkExtractItem is not null)
            {
                context.Remove(linkExtractItem);
            }
        }

        private async Task AddParcelLink(
            ExtractContext context,
            Guid parcelId,
            string caPaKey,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var newAddress = await context
                .ParcelLinkExtract
                .FindAsync(new object?[] { parcelId, addressPersistentLocalId }, ct);

            if (newAddress is null || context.Entry(newAddress).State == EntityState.Deleted)
            {
                await context
                    .ParcelLinkExtract
                    .AddAsync(new ParcelLinkExtractItem
                    {
                        ParcelId = parcelId,
                        CaPaKey = caPaKey,
                        AddressPersistentLocalId = addressPersistentLocalId,
                        Count = 1,
                        DbaseRecord = new ParcelLinkDbaseRecord
                        {
                            objecttype = { Value = ParcelObjectType },
                            adresobjid = { Value = caPaKey },
                            adresid = { Value = addressPersistentLocalId }
                        }.ToBytes(_encoding)
                    }, ct);
            }
        }

        private static Task DoNothing<T>(ExtractContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
