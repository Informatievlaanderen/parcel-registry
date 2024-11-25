namespace ParcelRegistry.Projections.BackOffice
{
    using System.Threading;
    using System;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Parcel;
    using Parcel.Events;

    public class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory, IConfiguration configuration)
        {
            var delayInSeconds = configuration.GetValue("DelayInSeconds", 10);

            When<Envelope<ParcelWasMigrated>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await backOfficeContext.AddIdempotentParcelAddressRelation(
                        new ParcelId(message.Message.ParcelId),
                        new AddressPersistentLocalId(addressPersistentLocalId),
                        cancellationToken);

                    await backOfficeContext.SaveChangesAsync(cancellationToken);
                }
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);

                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId),
                    cancellationToken,
                    saveChanges: false);

                await backOfficeContext.AddIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.NewAddressPersistentLocalId),
                    cancellationToken,
                    saveChanges: false);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);

                var previousAddress = await backOfficeContext.FindParcelAddressRelation(
                    new ParcelId(message.Message.ParcelId),
                    new AddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId),
                    cancellationToken);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    backOfficeContext.ParcelAddressRelations.Remove(previousAddress);
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
                    await backOfficeContext.ParcelAddressRelations.AddAsync(
                        new ParcelAddressRelation(message.Message.ParcelId, message.Message.NewAddressPersistentLocalId),
                        cancellationToken);
                }
                else
                {
                    newAddress.Count += 1;
                }

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<ParcelWasImported>>(DoNothing);
            When<Envelope<ParcelGeometryWasChanged>>(DoNothing);
            When<Envelope<ParcelAddressesWereReaddressed>>(DoNothing);
            When<Envelope<ParcelWasRetiredV2>>(DoNothing);
            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(DoNothing);
        }

        private static async Task DelayProjection<TMessage>(Envelope<TMessage> envelope, int delayInSeconds, CancellationToken cancellationToken)
            where TMessage : IMessage
        {
            var differenceInSeconds = (DateTime.UtcNow - envelope.CreatedUtc).TotalSeconds;
            if (differenceInSeconds < delayInSeconds)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds - differenceInSeconds), cancellationToken);
            }
        }

        private static Task DoNothing<T>(BackOfficeProjectionsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
