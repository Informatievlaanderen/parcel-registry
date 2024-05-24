namespace ParcelRegistry.Infrastructure.Repositories
{
    using AllStream;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Parcel;
    using SqlStreamStore;

    public class AllStreamRepository : Repository<AllStream, AllStreamId>, IAllStreamRepository
    {
        public AllStreamRepository(
            ConcurrentUnitOfWork unitOfWork,
            IStreamStore eventStore,
            EventMapping eventMapping,
            EventDeserializer eventDeserializer)
            : base(() => new AllStream(), unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}
