namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using DataStructures;
    using Events;
    using Exceptions;

    public sealed partial class Parcel : AggregateRootEntity, ISnapshotable
    {
        public static Parcel MigrateParcel(
            IParcelFactory parcelFactory,
            Legacy.ParcelId oldParcelId,
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
                    oldParcelId,
                    parcelId,
                    caPaKey,
                    parcelStatus,
                    isRemoved,
                    addressPersistentLocalIds,
                    xCoordinate,
                    yCoordinate));

            return newParcel;
        }

        public void AttachAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (ParcelStatus != ParcelStatus.Realized)
            {
                throw new ParcelHasInvalidStatusException();
            }

            if(AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            var address = _addresses.GetOptional(addressPersistentLocalId);

            if (address is null)
            {
                throw new AddressNotFoundException();
            }

            if (address.Value.IsRemoved)
            {
                throw new AddressIsRemovedException();
            }

            var validStatuses = new[] { AddressStatus.Current, AddressStatus.Proposed };

            if (!validStatuses.Contains(address.Value.Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            ApplyChange(new ParcelAddressWasAttachedV2(ParcelId, addressPersistentLocalId));
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
                CaPaKey,
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
