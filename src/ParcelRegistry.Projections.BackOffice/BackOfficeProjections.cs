namespace ParcelRegistry.Projections.BackOffice
{
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Parcel;
    using Parcel.Events;

    public class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            When<Envelope<ParcelWasMigrated>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await backOfficeContext.AddIdempotentParcelAddressRelation(
                        new ParcelId(message.Message.ParcelId),
                        new AddressPersistentLocalId(addressPersistentLocalId),
                        cancellationToken);
                }
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);

                var previousAddress = await backOfficeContext.FindParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId),
                    cancellationToken);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                        new ParcelId(message.Message.ParcelId),
                        new AddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId),
                        cancellationToken);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress = await backOfficeContext.FindParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.NewAddressPersistentLocalId),
                    cancellationToken);

                if (newAddress is null)
                {
                    await backOfficeContext.AddIdempotentParcelAddressRelation(
                        new ParcelId(message.Message.ParcelId),
                        new AddressPersistentLocalId(message.Message.NewAddressPersistentLocalId),
                        cancellationToken);
                }
                else
                {
                    newAddress.Count += 1;
                }
            });
        }
    }
}
