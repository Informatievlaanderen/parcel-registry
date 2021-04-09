namespace ParcelRegistry.Tests.SnapshotTests
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Parcel;

    public class ParcelFake : Parcel
    {
        public new static readonly Func<Parcel> Factory = () => new ParcelFake();

        public override ISnapshotStrategy Strategy => IntervalStrategy.SnapshotEvery(1);
    }
}
