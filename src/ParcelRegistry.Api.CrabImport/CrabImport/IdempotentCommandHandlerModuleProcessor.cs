namespace ParcelRegistry.Api.CrabImport.CrabImport
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using SqlStreamStore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Legacy;
    using Legacy.Commands.Crab;
    using Legacy.Commands.Fixes;

    public class IdempotentCommandHandlerModuleProcessor : IIdempotentCommandHandlerModuleProcessor
    {
        private readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly ParcelCommandHandlerModule _parcelCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, Parcel, Provenance> _provenanceFactory;
        private readonly Func<IParcels> _getParcels;
        private readonly IParcelFactory _parcelFactory;
        private readonly Func<FixGrar1475, Parcel, Provenance> _fixGrar1475ProvenanceFactory;
        private readonly Func<FixGrar1637, Parcel, Provenance> _fixGrar1637ProvenanceFactory;
        private readonly Func<FixGrar3581, Parcel, Provenance> _fixGrar3581ProvenanceFactory;

        public IdempotentCommandHandlerModuleProcessor(
            Func<IParcels> getParcels,
            IParcelFactory parcelFactory,
            ConcurrentUnitOfWork concurrentUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            LegacyProvenanceFactory legacyProvenanceFactory,
            IProvenanceFactory<Parcel> provenanceFactory)
        {
            _getParcels = getParcels;
            _parcelFactory = parcelFactory;
            _concurrentUnitOfWork = concurrentUnitOfWork;
            _provenanceFactory = legacyProvenanceFactory.CreateFrom;

            var fixGrar1475ProvenanceFactory = new FixGrar1475ProvenanceFactory();
            _fixGrar1475ProvenanceFactory = fixGrar1475ProvenanceFactory.CreateFrom;

            var fixGrar1637ProvenanceFactory = new FixGrar1637ProvenanceFactory();
            _fixGrar1637ProvenanceFactory = fixGrar1637ProvenanceFactory.CreateFrom;

            var fixGrar3581ProvenanceFactory = new FixGrar3581ProvenanceFactory();
            _fixGrar3581ProvenanceFactory = fixGrar3581ProvenanceFactory.CreateFrom;

            _parcelCommandHandlerModule = new ParcelCommandHandlerModule(
                getParcels,
                parcelFactory,
                () => concurrentUnitOfWork,
                getStreamStore,
                eventMapping,
                eventSerializer,
                legacyProvenanceFactory,
                provenanceFactory,
                fixGrar1475ProvenanceFactory,
                fixGrar1637ProvenanceFactory,
                fixGrar3581ProvenanceFactory);
        }

        public async Task<CommandMessage> Process(
           dynamic commandToProcess,
           IDictionary<string, object> metadata,
           int currentPosition,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (commandToProcess)
            {
                case ImportTerrainObjectFromCrab command:
                    var commandTerrainObject = new CommandMessage<ImportTerrainObjectFromCrab>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.ImportTerrainObject(_getParcels, _parcelFactory, commandTerrainObject, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandTerrainObject, _provenanceFactory, currentPosition);
                    return commandTerrainObject;

                case ImportTerrainObjectHouseNumberFromCrab command:
                    var commandTerrainObjectHouseNumber = new CommandMessage<ImportTerrainObjectHouseNumberFromCrab>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.ImportTerrainObjectHouseNumber(_getParcels, commandTerrainObjectHouseNumber, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandTerrainObjectHouseNumber, _provenanceFactory, currentPosition);
                    return commandTerrainObjectHouseNumber;

                case ImportSubaddressFromCrab command:
                    var commandSubaddress = new CommandMessage<ImportSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.ImportSubaddress(_getParcels, commandSubaddress, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddress, _provenanceFactory, currentPosition);
                    return commandSubaddress;

                case FixGrar1475 command:
                    var commandFixGrar1475 = new CommandMessage<FixGrar1475>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.FixGrar1475(_getParcels, commandFixGrar1475, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandFixGrar1475, _fixGrar1475ProvenanceFactory, currentPosition);
                    return commandFixGrar1475;

                case FixGrar1637 command:
                    var commandFixGrar1637 = new CommandMessage<FixGrar1637>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.FixGrar1637(_getParcels, commandFixGrar1637, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandFixGrar1637, _fixGrar1637ProvenanceFactory, currentPosition);
                    return commandFixGrar1637;

                case FixGrar3581 command:
                    var commandFixGrar3581 = new CommandMessage<FixGrar3581>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.FixGrar3581(_getParcels, commandFixGrar3581, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandFixGrar3581, _fixGrar3581ProvenanceFactory, currentPosition);
                    return commandFixGrar3581;

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }
        }
    }
}
