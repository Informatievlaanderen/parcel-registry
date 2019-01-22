namespace ParcelRegistry.Api.CrabImport.CrabImport
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands.Crab;

    public class IdempotentCommandHandlerModuleProcessor : IIdempotentCommandHandlerModuleProcessor
    {
        private readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly ParcelCommandHandlerModule _parcelCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, Parcel, Provenance> _provenanceFactory = new ParcelProvenanceFactory().CreateFrom;
        private readonly Func<IParcels> _getParcels;

        public IdempotentCommandHandlerModuleProcessor(
            IComponentContext container,
            ConcurrentUnitOfWork concurrentUnitOfWork)
        {
            _concurrentUnitOfWork = concurrentUnitOfWork;
            _getParcels = container.Resolve<Func<IParcels>>();
            _parcelCommandHandlerModule = new ParcelCommandHandlerModule(_getParcels, () => concurrentUnitOfWork);
        }

        public async Task<CommandMessage> Process(
           dynamic commandToProcess,
           IDictionary<string, object> metadata,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (commandToProcess)
            {
                case ImportTerrainObjectFromCrab command:
                    var commandTerrainObject = new CommandMessage<ImportTerrainObjectFromCrab>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.ImportTerrainObject(_getParcels, commandTerrainObject, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandTerrainObject, _provenanceFactory);
                    return commandTerrainObject;

                case ImportTerrainObjectHouseNumberFromCrab command:
                    var commandTerrainObjectHouseNumber = new CommandMessage<ImportTerrainObjectHouseNumberFromCrab>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.ImportTerrainObjectHouseNumber(_getParcels, commandTerrainObjectHouseNumber, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandTerrainObjectHouseNumber, _provenanceFactory);
                    return commandTerrainObjectHouseNumber;

                case ImportSubaddressFromCrab command:
                    var commandSubaddress = new CommandMessage<ImportSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _parcelCommandHandlerModule.ImportSubaddress(_getParcels, commandSubaddress, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddress, _provenanceFactory);
                    return commandSubaddress;

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }
        }
    }
}
