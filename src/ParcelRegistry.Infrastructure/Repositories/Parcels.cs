namespace ParcelRegistry.Infrastructure.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Parcel;
    using SqlStreamStore;

    public class Parcels : Repository<Parcel, ParcelStreamId>, IParcels
    {
        public Parcels(IParcelFactory parcelFactory, ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(parcelFactory.Create, unitOfWork, eventStore, eventMapping, eventDeserializer) { }

        public async Task<string> GetHash(ParcelId parcelId, CancellationToken cancellationToken)
        {
            var aggregate = await GetAsync(new ParcelStreamId(parcelId), cancellationToken);

            return aggregate.LastEventHash;
        }
    }
}
