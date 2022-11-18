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

        public ParcelFactory(ISnapshotStrategy snapshotStrategy)
        {
            _snapshotStrategy = snapshotStrategy;
        }

        public Parcel Create()
        {
            return new Parcel(_snapshotStrategy);
        }
    }
}
