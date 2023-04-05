namespace ParcelRegistry.Projections.Extract.ParcelLinkExtract
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.Extensions.Options;
    using Parcel.Events;

    [ConnectedProjectionName("Extract perceelkoppelingen")]
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
                        .ParcelLinkExtract
                        .AddAsync(new ParcelLinkExtractItem
                        {
                            ParcelId = message.Message.ParcelId,
                            CaPaKey = message.Message.CaPaKey,
                            AddressPersistentLocalId = addressPersistentLocalId,
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
                await RemoveParcelLink(context, message.Message.ParcelId, message.Message.PreviousAddressPersistentLocalId, ct);

                await context
                    .ParcelLinkExtract
                    .AddAsync(new ParcelLinkExtractItem
                    {
                        ParcelId = message.Message.ParcelId,
                        CaPaKey = message.Message.CaPaKey,
                        AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                        DbaseRecord = new ParcelLinkDbaseRecord
                        {
                            objecttype = { Value = ParcelObjectType },
                            adresobjid = { Value = message.Message.CaPaKey },
                            adresid = { Value = message.Message.AddressPersistentLocalId }
                        }.ToBytes(_encoding)
                    }, ct);
            });
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

            context.Remove(linkExtractItem);
        }
    }
}
