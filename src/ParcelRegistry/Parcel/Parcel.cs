namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
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

        public static Parcel ImportParcel(
            IParcelFactory parcelFactory,
            VbrCaPaKey vbrCaPaKey,
            ParcelId parcelId,
            ExtendedWkbGeometry extendedWkbGeometry,
            List<AddressPersistentLocalId> addressesToAttach)
        {
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            var newParcel = parcelFactory.Create();

            newParcel.ApplyChange(
                new ParcelWasImported(
                    parcelId,
                    vbrCaPaKey,
                    extendedWkbGeometry));

            foreach (var address in addressesToAttach)
            {
                newParcel.ApplyChange(
                    new ParcelAddressWasAttachedV2(
                        parcelId,
                        vbrCaPaKey,
                        address));
            }

            return newParcel;
        }

        public void CorrectRetirement(
            VbrCaPaKey vbrCaPaKey,
            ParcelId parcelId,
            ExtendedWkbGeometry extendedWkbGeometry,
            List<AddressPersistentLocalId> addressesToAttach)
        {
            GuardParcelNotRemoved();
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            ApplyChange(
                new ParcelWasCorrectedFromRetiredToRealized(
                    parcelId,
                    vbrCaPaKey,
                    extendedWkbGeometry));

            foreach (var address in addressesToAttach)
            {
                ApplyChange(
                    new ParcelAddressWasAttachedV2(
                        parcelId,
                        vbrCaPaKey,
                        address));
            }
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

        public void ChangeGeometry(ExtendedWkbGeometry extendedWkbGeometry, List<AddressPersistentLocalId> addresses)
        {
            GuardParcelNotRemoved();
            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            if (Geometry == extendedWkbGeometry)
            {
                return;
            }

            var addressesToDetach = _addressPersistentLocalIds.Except(addresses).ToList();
            var addressesToAttach = addresses.Except(_addressPersistentLocalIds).ToList();

            foreach (var address in addressesToDetach)
            {
                ApplyChange(new ParcelAddressWasDetachedV2(
                    ParcelId,
                    CaPaKey,
                    address));
            }

            foreach (var address in addressesToAttach)
            {
                ApplyChange(new ParcelAddressWasAttachedV2(
                    ParcelId,
                    CaPaKey,
                    address));
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
