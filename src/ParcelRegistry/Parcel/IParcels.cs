namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IParcels : IAsyncRepository<Parcel, ParcelStreamId>
    {
        Task<string> GetHash(ParcelId parcelId, CancellationToken cancellationToken);
    }

    public class ParcelStreamId : ValueObject<ParcelStreamId>
    {
        private readonly ParcelId _parcelId;

        public ParcelStreamId(ParcelId parcelId)
        {
            _parcelId = parcelId;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return _parcelId;
        }

        public override string ToString() => $"parcel-{_parcelId}";
    }
}
