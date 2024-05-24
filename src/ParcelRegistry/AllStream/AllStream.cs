namespace ParcelRegistry.AllStream
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Events;
    using Parcel;

    public sealed class AllStream : AggregateRootEntity
    {
        public void CreateOsloSnapshots(IReadOnlyList<VbrCaPaKey> caPaKeys)
        {
            ApplyChange(new ParcelOsloSnapshotsWereRequested(
                caPaKeys.ToDictionary(ParcelId.CreateFor, x => x)));
        }
    }
}
