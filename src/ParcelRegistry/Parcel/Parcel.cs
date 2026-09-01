namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
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
            GuardPolygon(WKBReaderFactory.CreateForLambert72().Read(extendedWkbGeometry));

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
            GuardPolygon(WKBReaderFactory.CreateForLambert72().Read(extendedWkbGeometry));

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
            GuardPolygon(WKBReaderFactory.CreateForLambert72().Read(extendedWkbGeometry));

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
            GuardPolygon(WKBReaderFactory.CreateForLambert72().Read(extendedWkbGeometry));

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

        /// <summary>
        /// Re-expresses the geometry in Lambert 2008 (EPSG 3812) for the one-off event store transformation,
        /// see ADR 0005.
        /// </summary>
        /// <remarks>
        /// Deliberately unguarded: unlike <see cref="ChangeGeometry"/> this is not an edit of the parcel but a
        /// change of the reference system its geometry is expressed in, and it has to reach every parcel the
        /// event store holds — removed and retired ones included — or the event store would be left holding both
        /// reference systems forever. <see cref="GuardPolygon"/> is not run either: it pins the geometry to
        /// Lambert 72, which is the very thing this leaves behind.
        ///
        /// A geometry that is already Lambert 2008 applies nothing, which is what makes re-running the
        /// transformation over a stream a no-op instead of a double transform.
        /// </remarks>
        public void TransformToLambert2008()
        {
            var extendedWkb = Geometry.ToString().ToByteArray();

            // Qualified: the GrAr WKBReaderFactory this file already uses for CreateForLambert72 throws on
            // SRID-less bytes, and geometries persisted before the event store wrote EWKB are exactly that.
            var geometry = ParcelRegistry.WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);

            if (geometry.SRID == SystemReferenceId.SridLambert2008)
            {
                return;
            }

            // The explicit transform rather than EnsureLambert08: that one relabels geometries falling outside
            // Flanders instead of transforming them, which would silently corrupt any geometry this touches.
            // Rounded to 2 decimals, the centimetre precision geometries are persisted at.
            var transformed = geometry.TransformFromLambert72To08(roundingPrecision: 2);

            ApplyChange(new ParcelGeometryCrsWasChanged(
                ParcelId,
                CaPaKey,
                ExtendedWkbGeometry.Create(transformed)));
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
