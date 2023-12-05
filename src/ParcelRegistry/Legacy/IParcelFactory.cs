namespace ParcelRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;

    [Obsolete("This is a legacy interface and should not be used anymore.")]
    public interface IParcelFactory
    {
        public Parcel Create();
    }

    [Obsolete("This is a legacy class and should not be used anymore.")]
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
