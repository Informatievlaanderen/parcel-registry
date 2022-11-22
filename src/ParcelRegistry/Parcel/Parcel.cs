namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;

    public sealed partial class Parcel : AggregateRootEntity, ISnapshotable
    {
        public static Parcel MigrateParcel(IParcelFactory parcelFactory,
            ParcelId parcelId,
            VbrCaPaKey caPaKey,
            ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds,
            Coordinate? xCoordinate,
            Coordinate? yCoordinate)
        {
            var newParcel = parcelFactory.Create();
            newParcel.ApplyChange(
                new ParcelWasMigrated(
                    parcelId,
                    caPaKey,
                    parcelStatus,
                    isRemoved,
                    addressPersistentLocalIds,
                    xCoordinate,
                    yCoordinate));

            return newParcel;
        }

        #region Metadata

        protected override void BeforeApplyChange(object @event)
        {
            _ = new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion

        #region Snapshot

        public object TakeSnapshot()
        {
            return new ParcelSnapshotV2(
                ParcelId,
                ParcelStatus,
                IsRemoved,
                _addressPersistentLocalIds,
                XCoordinate,
                YCoordinate,
                LastEventHash,
                LastProvenanceData);
        }

        public ISnapshotStrategy Strategy { get; }

        #endregion
    }
}
