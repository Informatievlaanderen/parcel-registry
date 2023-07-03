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
            Coordinate? xCoordinate,
            Coordinate? yCoordinate,
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
                    xCoordinate,
                    yCoordinate,
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

        public void ImportParcelGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            ;
            GuardParcelNotRemoved();
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            if (Geometry == extendedWkbGeometry)
                return;

            ApplyChange(new ParcelGeometryWasImported(
                ParcelId,
                CaPaKey,
                extendedWkbGeometry));
        }

        private static void GuardPolygon(Geometry? geometry)
        {
            if (geometry is Polygon
                && (geometry.SRID != ExtendedWkbGeometry.SridLambert72 || !GeometryValidator.IsValid(geometry)))
            {
                throw new PolygonIsInvalidException();
            }

            if (geometry is MultiPolygon multiPolygon
                && (multiPolygon.SRID != ExtendedWkbGeometry.SridLambert72 ||
                    multiPolygon.Geometries.Any(polygon => !GeometryValidator.IsValid(polygon))))
            {
                throw new PolygonIsInvalidException();
            }
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
                XCoordinate,
                YCoordinate,
                LastEventHash,
                LastProvenanceData);
        }

        public ISnapshotStrategy Strategy { get; }

        #endregion
    }
}
