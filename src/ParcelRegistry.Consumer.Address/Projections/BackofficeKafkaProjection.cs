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
            });

            When<AddressWasProposedV2>(async (context, message, ct) =>
            {
                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed)
                        , ct);
            });

            When<AddressWasApproved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });

            When<AddressWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseHouseNumberWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRetiredV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRemovedV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
            });

            When<AddressWasRemovedBecauseHouseNumberWasRemoved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
            });

            When<AddressWasDeregulated>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });

            When<AddressWasCorrectedFromApprovedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromRejectedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromRetiredToCurrent>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });
        }
    }
}
