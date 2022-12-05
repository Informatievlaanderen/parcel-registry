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
    using NodaTime.Text;
    using Parcel;
    using Parcel.Commands;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Provenance;

    public class CommandHandlingKafkaProjection : ConnectedProjection<CommandHandler>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public CommandHandlingKafkaProjection(BackOfficeContext backOfficeContext)
        {
            _backOfficeContext = backOfficeContext;

            When<AddressWasRemovedV2>(async (commandHandler, message, ct) =>
            {
                var relations = backOfficeContext.ParcelAddressRelations
                    .Where(x => x.AddressPersistentLocalId == new AddressPersistentLocalId(message.AddressPersistentLocalId))
                    .ToList();

                foreach (var relation in relations)
                {
                    var command = new DetachAddressBecauseAddressWasRemoved(
                        new ParcelId(relation.ParcelId),
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        FromProvenance(message.Provenance));
                    await commandHandler.Handle(command, ct);
                }
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

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
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

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });
        }

        private async Task DetachBecauseRetired(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            var relations = _backOfficeContext.ParcelAddressRelations
                .Where(x => x.AddressPersistentLocalId == new AddressPersistentLocalId(addressPersistentLocalId))
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressBecauseAddressWasRetired(
                    new ParcelId(relation.ParcelId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);
            }
        }

        private async Task DetachBecauseRejected(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            var relations = _backOfficeContext.ParcelAddressRelations
                .Where(x => x.AddressPersistentLocalId == new AddressPersistentLocalId(addressPersistentLocalId))
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressBecauseAddressWasRejected(
                    new ParcelId(relation.ParcelId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);
            }
        }

        private static Provenance FromProvenance(Contracts.Provenance provenance) =>
            new Provenance(
                InstantPattern.General.Parse(provenance.Timestamp).GetValueOrThrow(),
                Enum.Parse<Application>(provenance.Application),
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                Enum.Parse<Modification>(provenance.Modification),
                Enum.Parse<Organisation>(provenance.Organisation));
    }
}
