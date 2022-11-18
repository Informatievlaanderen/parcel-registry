namespace ParcelRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Legacy;
    using SqlStreamStore;

    public class LegacyParcels : Repository<Parcel, ParcelId>, IParcels
    {
        public LegacyParcels(IParcelFactory parcelFactory, ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(parcelFactory.Create, unitOfWork, eventStore, eventMapping, eventDeserializer) { }
    }
}
