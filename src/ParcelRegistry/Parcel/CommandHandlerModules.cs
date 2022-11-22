namespace ParcelRegistry.Parcel
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using SqlStreamStore;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<ProvenanceFactory<Parcel>>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder
                .RegisterType<ParcelCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(ParcelCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }

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
                    var streamId = new ParcelStreamId(message.Command.ParcelId);
                    var parcel = await parcelRepository().GetOptionalAsync(streamId, ct);

                    if (parcel.HasValue)
                    {
                        throw new AggregateSourceException($"Parcel with id {message.Command.ParcelId} already exists");
                    }

                    var newParcel = Parcel.MigrateParcel(
                        parcelFactory,
                        message.Command.ParcelId,
                        message.Command.CaPaKey,
                        message.Command.ParcelStatus,
                        message.Command.IsRemoved,
                        message.Command.AddressPersistentLocalIds,
                        message.Command.XCoordinate,
                        message.Command.YCoordinate);

                    parcelRepository().Add(streamId, newParcel);
                });
        }
    }
}
