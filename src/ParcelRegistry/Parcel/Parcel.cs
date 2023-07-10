namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using DataStructures;
    using Events;
    using Exceptions;
    using NetTopologySuite.Geometries;

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
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            var newParcel = parcelFactory.Create();
            newParcel.ApplyChange(
                new ParcelWasMigrated(
                    oldParcelId,
                    parcelId,
                    caPaKey,
                    parcelStatus,
                    isRemoved,
                    addressPersistentLocalIds,
                    extendedWkbGeometry));

            return newParcel;
        }

        public void AttachAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardParcelNotRemoved();

            if (ParcelStatus != ParcelStatus.Realized)
            {
                throw new ParcelHasInvalidStatusException();
            }

            if (AddressPersistentLocalIds.Contains(addressPersistentLocalId))
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

            ApplyChange(new ParcelAddressWasAttachedV2(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardParcelNotRemoved();

            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedV2(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRemoved(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedBecauseAddressWasRemoved(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRejected(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedBecauseAddressWasRejected(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRetired(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedBecauseAddressWasRetired(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void ReplaceAttachedAddressBecauseAddressWasReaddressed(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId)
        {
            if (AddressPersistentLocalIds.Contains(addressPersistentLocalId)
                && !AddressPersistentLocalIds.Contains(previousAddressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                ParcelId,
                CaPaKey,
                addressPersistentLocalId,
                previousAddressPersistentLocalId));
        }

        public static Parcel ImportParcel(
            IParcelFactory parcelFactory,
            VbrCaPaKey vbrCaPaKey,
            ParcelId parcelId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            var newParcel = parcelFactory.Create();

            newParcel.ApplyChange(
                new ParcelWasImported(
                    parcelId,
                    vbrCaPaKey,
                    extendedWkbGeometry));

            return newParcel;
        }

        public void RetireParcel()
        {
            GuardParcelNotRemoved();

            if (ParcelStatus == ParcelStatus.Retired)
            {
                return;
            }

            foreach (var address in _addressPersistentLocalIds.ToList())
            {
                ApplyChange(new ParcelAddressWasDetachedV2(ParcelId, CaPaKey, address));
            }

            ApplyChange(new ParcelWasRetiredV2(ParcelId, CaPaKey));
        }

        public void ChangeGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            GuardParcelNotRemoved();
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            if (Geometry == extendedWkbGeometry)
            {
                return;
            }

            ApplyChange(new ParcelGeometryWasChanged(ParcelId, CaPaKey, extendedWkbGeometry));
        }

        private static void GuardPolygon(Geometry? geometry)
        {
            if (geometry is Polygon
                && geometry.SRID == ExtendedWkbGeometry.SridLambert72
                && GeometryValidator.IsValid(geometry))
            {
                return;
            }

            if (geometry is MultiPolygon multiPolygon
                && multiPolygon.SRID == ExtendedWkbGeometry.SridLambert72
                && multiPolygon.Geometries.All(GeometryValidator.IsValid))
            {
                return;
            }

            throw new PolygonIsInvalidException();
        }

        private void GuardParcelNotRemoved()
        {
            if (IsRemoved)
            {
                throw new ParcelIsRemovedException(ParcelId);
            }
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
                Geometry,
                LastEventHash,
                LastProvenanceData);
        }

        public ISnapshotStrategy Strategy { get; }

        #endregion
    }
}
