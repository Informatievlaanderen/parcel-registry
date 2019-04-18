namespace ParcelRegistry.Api.CrabImport.CrabImport
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands.Crab;
    using SqlStreamStore;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class IdempotentCommandHandlerModuleProcessor : IIdempotentCommandHandlerModuleProcessor
    {
        private readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly ParcelCommandHandlerModule _parcelCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, Parcel, Provenance> _provenanceFactory;
        private readonly Func<IParcels> _getParcels;

        public IdempotentCommandHandlerModuleProcessor(
            Func<IParcels> getParcels,
            ConcurrentUnitOfWork concurrentUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            ParcelProvenanceFactory provenanceFactory)
        {
            _getParcels = getParcels;
            _concurrentUnitOfWork = concurrentUnitOfWork;
            _provenanceFactory = provenanceFactory.CreateFrom;

            _parcelCommandHandlerModule = new ParcelCommandHandlerModule(
                getParcels,
                () => concurrentUnitOfWork,
                getStreamStore,
                eventMapping,
                eventSerializer,
                provenanceFactory);
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
                    await _parcelCommandHandlerModule.ImportTerrainObject(_getParcels, commandTerrainObject, cancellationToken);
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

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }
        }
    }
}
