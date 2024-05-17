namespace ParcelRegistry.Consumer.Address.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
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

        public CommandHandlingKafkaProjection(
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            IParcels parcels)
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

                var sourceAddressParcelRelations = await backOfficeContext.ParcelAddressRelations
                    .AsNoTracking()
                    .Where(x => sourceAddressPersistentLocalIds.Contains(x.AddressPersistentLocalId))
                    .ToListAsync(cancellationToken: ct);

                var commandByParcels = sourceAddressParcelRelations
                    .GroupBy(
                        relation => relation.ParcelId,
                        relation => readdresses.Where(x => x.SourceAddressPersistentLocalId == relation.AddressPersistentLocalId))
                    .Select(x => new ReaddressAddresses(
                        new ParcelId(x.Key),
                        x.SelectMany(a => a),
                        FromProvenance(message.Provenance)))
                    .ToList();

                foreach (var command in commandByParcels)
                {
                    try
                    {
                        await commandHandler.HandleIdempotent(command, ct);
                    }
                    catch (IdempotencyException)
                    {
                        // do nothing
                    }
                }

                await backOfficeContext.Database.BeginTransactionAsync();
                
                foreach (var parcelId in commandByParcels.Select(x => x.ParcelId))
                {
                    var parcel = await parcels.GetAsync(new ParcelStreamId(parcelId), ct);

                    var backOfficeAddresses = (await backOfficeContext.ParcelAddressRelations
                        .AsNoTracking()
                        .Where(x => x.ParcelId == parcelId)
                        .Select(x => x.AddressPersistentLocalId)
                        .ToListAsync(cancellationToken: ct))
                        .Select(x => new AddressPersistentLocalId(x))
                        .ToList();

                    var addressesToRemove = backOfficeAddresses.Except(parcel.AddressPersistentLocalIds).ToList();
                    var addressesToAdd = parcel.AddressPersistentLocalIds.Except(backOfficeAddresses).ToList();

                    foreach (var addressPersistentLocalId in addressesToRemove)
                    {
                        await backOfficeContext.RemoveIdempotentParcelAddressRelation(parcelId, addressPersistentLocalId, ct);
                    }

                    foreach (var addressPersistentLocalId in addressesToAdd)
                    {
                        await backOfficeContext.AddIdempotentParcelAddressRelation(parcelId, addressPersistentLocalId, ct);
                    }
                }

                await backOfficeContext.Database.CommitTransactionAsync();
            });

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
