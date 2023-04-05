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
    using SqlStreamStore;

    public sealed class AddressCommandHandlerModule : CommandHandlerModule
    {
        public AddressCommandHandlerModule(
            Func<IParcels> parcelRepository,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            Func<ISnapshotStore> getSnapshotStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IProvenanceFactory<Parcel> provenanceFactory)
        {
            For<DetachAddressBecauseAddressWasRemoved>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressBecauseAddressWasRemoved, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetAsync(streamId, ct);

                    parcel.DetachAddressBecauseAddressWasRemoved(message.Command.AddressPersistentLocalId);
                });

            For<DetachAddressBecauseAddressWasRejected>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressBecauseAddressWasRejected, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetAsync(streamId, ct);

                    parcel.DetachAddressBecauseAddressWasRejected(message.Command.AddressPersistentLocalId);
                });

            For<DetachAddressBecauseAddressWasRetired>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressBecauseAddressWasRetired, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetAsync(streamId, ct);

                    parcel.DetachAddressBecauseAddressWasRetired(message.Command.AddressPersistentLocalId);
                });

            For<ReplaceAttachedAddressBecauseAddressWasReaddressed>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ReplaceAttachedAddressBecauseAddressWasReaddressed, Parcel>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetAsync(streamId, ct);

                    parcel.ReplaceAttachedAddressBecauseAddressWasReaddressed(
                        message.Command.AddressPersistentLocalId,
                        message.Command.PreviousAddressPersistentLocalId);
                });
        }
    }
}
