namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;

    public interface IParcelFactory
    {
        Parcel Create();
    }

    public class ParcelFactory : IParcelFactory
    {
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly IAddresses _addresses;

        public ParcelFactory(ISnapshotStrategy snapshotStrategy, IAddresses addresses)
        {
            _snapshotStrategy = snapshotStrategy;
            _addresses = addresses;
        }

        public Parcel Create()
        {
            return new Parcel(_snapshotStrategy, _addresses);
        }
    }
}
