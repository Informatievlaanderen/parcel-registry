namespace ParcelRegistry.Consumer.Address.Projections
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class BackOfficeKafkaProjection : ConnectedProjection<ConsumerAddressContext>
    {
        private async Task CatchDbUpdateException(Func<Task> func, ConsumerAddressContext context)
        {
            try
            {
                await func();
            }
            catch (DbUpdateException ex)
            {
                const int uniqueConstraintExceptionCode = 2627;
                const int uniqueIndexExceptionCode = 2601;

                if (ex.InnerException is SqlException innerException
                    && innerException.Number is uniqueConstraintExceptionCode or uniqueIndexExceptionCode)
                {
                    // When the service crashes between EF ctx saveChanges and Kafka's commit offset
                    // it will try to reconsume the same message that was already saved to db causing duplicate key exception.
                    // In that case ignore.
                    context.ChangeTracker.Clear();
                }
                else
                {
                    throw;
                }
            }
        }

        public BackOfficeKafkaProjection()
        {
            When<AddressWasMigratedToStreetName>(async (context, message, ct) =>
            {
                await CatchDbUpdateException(async () =>
                {
                    await context
                        .AddressConsumerItems
                        .AddAsync(new AddressConsumerItem(
                                message.AddressPersistentLocalId,
                                Guid.Parse(message.AddressId),
                                AddressStatus.Parse(message.Status),
                                message.IsRemoved)
                            , ct);
                    await context.SaveChangesAsync(ct);
                }, context);
            });

            When<AddressWasProposedV2>(async (context, message, ct) =>
            {
                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed)
                        , ct);
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasApproved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasCorrectedFromApprovedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasDeregulated>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRejectedBecauseHouseNumberWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRejectedBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasCorrectedFromRejectedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRetiredV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRetiredBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasCorrectedFromRetiredToCurrent>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRemovedV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
                await context.SaveChangesAsync(ct);
            });

            When<AddressWasRemovedBecauseHouseNumberWasRemoved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
                await context.SaveChangesAsync(ct);
            });
        }
    }
}
