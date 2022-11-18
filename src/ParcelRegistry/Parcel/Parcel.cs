namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;

    public sealed partial class Parcel : AggregateRootEntity, ISnapshotable
    {
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
                ParcelPersistentLocalId,
                ParcelStatus,
                IsRemoved,
                _addressPersistentLocalIds,
                LastEventHash,
                LastProvenanceData);
        }

        public ISnapshotStrategy Strategy { get; }
        #endregion  
    }
}
