namespace ParcelRegistry.Consumer.Address.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Provenance;

    public sealed class CommandHandlingKafkaProjection : ConnectedProjection<CommandHandler>
    {
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;

        public CommandHandlingKafkaProjection(IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            _backOfficeContextFactory = backOfficeContextFactory;

            When<AddressWasMigratedToStreetName>(async (commandHandler, message, ct) =>
            {
                if (message.IsRemoved)
                {
                    await DetachBecauseRemoved(
                        commandHandler,
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        message.Provenance,
                        ct);
                }
                else if (message.Status == AddressStatus.Retired)
                {
                    await DetachBecauseRetired(
                        commandHandler,
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        message.Provenance,
                        ct);
                }
                else if (message.Status == AddressStatus.Rejected)
                {
                    await DetachBecauseRejected(
                        commandHandler,
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        message.Provenance,
                        ct);
                }
            });

            When<AddressWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseHouseNumberWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseHouseNumberWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseStreetNameWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredV2>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseHouseNumberWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseStreetNameWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRemovedV2>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRemoved(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRemovedBecauseHouseNumberWasRemoved>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRemoved(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRemovedBecauseStreetNameWasRemoved>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRemoved(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<StreetNameWasReaddressed>(async (commandHandler, message, ct) =>
            {
                await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);

                var readdresses = message.ReaddressedHouseNumbers
                    .Select(x => new ReaddressData(
                        new AddressPersistentLocalId(x.ReaddressedHouseNumber.SourceAddressPersistentLocalId),
                        new AddressPersistentLocalId(x.ReaddressedHouseNumber.DestinationAddressPersistentLocalId)))
                    .Concat(
                        message.ReaddressedHouseNumbers
                            .SelectMany(x => x.ReaddressedBoxNumbers)
                            .Select(boxNumberAddress => new ReaddressData(
                                new AddressPersistentLocalId(boxNumberAddress.SourceAddressPersistentLocalId),
                                new AddressPersistentLocalId(boxNumberAddress.DestinationAddressPersistentLocalId))))
                    .ToList();

                var sourceAddressPersistentLocalIds = readdresses
                    .Select(x => (int)x.SourceAddressPersistentLocalId)
                    .ToList();

                var parcelAddressRelations = await backOfficeContext.ParcelAddressRelations
                    .AsNoTracking()
                    .Where(x => sourceAddressPersistentLocalIds.Contains(x.AddressPersistentLocalId))
                    .ToListAsync(cancellationToken: ct);

                var commandByParcels = parcelAddressRelations
                    .GroupBy(
                        relation => relation.ParcelId,
                        relation => readdresses.Where(x => x.SourceAddressPersistentLocalId == relation.AddressPersistentLocalId))
                    .Select(x => new ReaddressAddresses(
                        new ParcelId(x.Key),
                        x.SelectMany(a => a),
                        FromProvenance(message.Provenance)));

                foreach (var command in commandByParcels)
                {
                    await commandHandler.Handle(command, ct);
                }
            });

            // When<AddressHouseNumberWasReaddressed>(async (commandHandler, message, ct) =>
            // {
            //     await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            //
            //     await ReplaceBecauseOfReaddress(
            //         commandHandler,
            //         backOfficeContext,
            //         message.ReaddressedHouseNumber,
            //         message.Provenance,
            //         ct);
            //
            //     foreach (var readdressedBoxNumber in message.ReaddressedBoxNumbers)
            //     {
            //         await ReplaceBecauseOfReaddress(
            //             commandHandler,
            //             backOfficeContext,
            //             readdressedBoxNumber,
            //             message.Provenance,
            //             ct);
            //     }
            // });

            When<AddressWasRejectedBecauseOfReaddress>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseOfReaddress>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });
        }

        private async Task DetachBecauseRemoved(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            var relations = backOfficeContext.ParcelAddressRelations
                .AsNoTracking()
                .Where(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressBecauseAddressWasRemoved(
                    new ParcelId(relation.ParcelId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);

                await backOfficeContext.RemoveIdempotentParcelAddressRelation(command.ParcelId, command.AddressPersistentLocalId, ct);
            }
        }

        private async Task DetachBecauseRetired(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            var relations = backOfficeContext.ParcelAddressRelations
                .AsNoTracking()
                .Where(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressBecauseAddressWasRetired(
                    new ParcelId(relation.ParcelId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);

                await backOfficeContext.RemoveIdempotentParcelAddressRelation(command.ParcelId, command.AddressPersistentLocalId, ct);
            }
        }

        private async Task DetachBecauseRejected(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            var relations = backOfficeContext.ParcelAddressRelations
                .AsNoTracking()
                .Where(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressBecauseAddressWasRejected(
                    new ParcelId(relation.ParcelId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);

                await backOfficeContext.RemoveIdempotentParcelAddressRelation(command.ParcelId, command.AddressPersistentLocalId, ct);
            }
        }

        private static Provenance FromProvenance(Contracts.Provenance provenance) =>
            new Provenance(
                SystemClock.Instance.GetCurrentInstant(),
                Enum.Parse<Application>(Application.AddressRegistry.ToString()),
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                Enum.Parse<Modification>(Modification.Update.ToString()),
                Enum.Parse<Organisation>(provenance.Organisation));
    }
}
