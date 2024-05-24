namespace ParcelRegistry.AllStream
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using SqlStreamStore;

    public sealed class AllStreamCommandHandlerModule : CommandHandlerModule
    {
        public AllStreamCommandHandlerModule(
            Func<IAllStreamRepository> allStreamRepository,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IProvenanceFactory<AllStream> provenanceFactory)
        {

            For<CreateOsloSnapshots>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var optionalAllStream = await allStreamRepository().GetOptionalAsync(AllStreamId.Instance, ct);

                    var allStream = optionalAllStream.HasValue ? optionalAllStream.Value : new AllStream();

                    allStream.CreateOsloSnapshots(message.Command.CaPaKeys);

                    if (!optionalAllStream.HasValue)
                    {
                        allStreamRepository().Add(AllStreamId.Instance, allStream);
                    }
                });
        }
    }
}
