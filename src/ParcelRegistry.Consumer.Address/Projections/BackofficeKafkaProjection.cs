namespace ParcelRegistry.Consumer.Address.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public sealed class BackOfficeKafkaProjection : ConnectedProjection<ConsumerAddressContext>
    {
        public BackOfficeKafkaProjection()
        {
            When<AddressWasMigratedToStreetName>(async (context, message, ct) =>
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
