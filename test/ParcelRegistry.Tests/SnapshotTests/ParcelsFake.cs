namespace ParcelRegistry.Tests.SnapshotTests
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Parcel;
    using SqlStreamStore;

    public class ParcelsFake : Repository<Parcel, ParcelId>, IParcels
    {
        public ParcelsFake(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(ParcelFake.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}
