namespace ParcelRegistry.Projections.Extract.ParcelLinkExtractWithCount
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Options;
    using Parcel.Events;

    [ConnectedProjectionName("Extract perceelkoppelingen met adres")]
    [ConnectedProjectionDescription("Projectie die de perceel koppelingen data voor het adreskoppelingen extract voorziet.")]
    public sealed class ParcelLinkExtractProjections : ConnectedProjection<ExtractContext>
    {
        public const string ParcelObjectType = "Perceel";

        private readonly Encoding _encoding;

        public ParcelLinkExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding)
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
                        .ParcelLinkExtractWithCount
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
                    .ParcelLinkExtractWithCount
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
                    .ParcelLinkExtractWithCount
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId }, ct);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    context.ParcelLinkExtractWithCount.Remove(previousAddress);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress = await context
                    .ParcelLinkExtractWithCount
                    .FindAsync(new object?[] { message.Message.ParcelId, message.Message.NewAddressPersistentLocalId }, ct);

                if (newAddress is null)
                {
                    await context
                        .ParcelLinkExtractWithCount
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
        }

        private static async Task RemoveParcelLink(
            ExtractContext context,
            Guid parcelId,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var linkExtractItem = await context
                .ParcelLinkExtractWithCount
                .FindAsync(new object?[] { parcelId, addressPersistentLocalId }, ct);

            context.Remove(linkExtractItem!);
        }
    }
}
