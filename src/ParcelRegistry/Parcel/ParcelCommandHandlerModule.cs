namespace ParcelRegistry.Parcel
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using Exceptions;
    using SqlStreamStore;

    public sealed class ParcelCommandHandlerModule : CommandHandlerModule
    {
        public ParcelCommandHandlerModule(
            IParcelFactory parcelFactory,
            Func<IParcels> parcelRepository,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            Func<ISnapshotStore> getSnapshotStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IProvenanceFactory<Parcel> provenanceFactory)
        {
            For<MigrateParcel>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<MigrateParcel, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.NewParcelId);
                    var parcel = await parcelRepository().GetOptionalAsync(streamId, ct);

                    if (parcel.HasValue)
                    {
                        throw new AggregateSourceException($"Parcel with id {message.Command.NewParcelId} already exists");
                    }

                    var newParcel = Parcel.MigrateParcel(
                        parcelFactory,
                        message.Command.OldParcelId,
                        message.Command.NewParcelId,
                        message.Command.CaPaKey,
                        message.Command.ParcelStatus,
                        message.Command.IsRemoved,
                        message.Command.AddressPersistentLocalIds,
                        message.Command.ExtendedWkbGeometry);

                    parcelRepository().Add(streamId, newParcel);
                });

            For<AttachAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<AttachAddress, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetAsync(streamId, ct);

                    parcel.AttachAddress(message.Command.AddressPersistentLocalId);
                });

            For<DetachAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddress, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetAsync(streamId, ct);

                    parcel.DetachAddress(message.Command.AddressPersistentLocalId);
                });

            For<ImportParcel>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ImportParcel, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);

                    var parcel = await parcelRepository().GetOptionalAsync(streamId, ct); // TODO: POSSIBLE AGGREGATE NOT FOUND

                     if (parcel.HasValue)
                     {
                         throw new ParcelAlreadyExistsException(message.Command.VbrCaPaKey);
                     }

                    var createdParcel = Parcel.ImportParcel(parcelFactory, message.Command.VbrCaPaKey, message.Command.ParcelId, message.Command.ExtendedWkbGeometry);
                    parcelRepository().Add(new ParcelStreamId(message.Command.ParcelId), createdParcel);
                });
        }
    }
}
