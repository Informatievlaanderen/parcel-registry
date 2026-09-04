namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
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
            GuardPolygon(ReadGeometry(extendedWkbGeometry));

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
            GuardPolygon(ReadGeometry(extendedWkbGeometry));

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
            GuardPolygon(ReadGeometry(extendedWkbGeometry));

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
            GuardPolygon(ReadGeometry(extendedWkbGeometry));

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
        /// reference systems forever. <see cref="GuardPolygon"/> is not run either: this changes nothing about
        /// the shape it already accepted.
        ///
        /// A geometry that is already Lambert 2008 applies nothing, which is what makes re-running the
        /// transformation over a stream a no-op instead of a double transform.
        /// </remarks>
        public void TransformToLambert2008()
        {
            var geometry = ReadGeometry(Geometry);

            if (geometry.SRID == SystemReferenceId.SridLambert2008)
            {
                return;
            }

            // Through the shared transformation, so a geometry converted here and the same geometry normalized
            // on the way in from GRB come out byte-for-byte identical.
            var transformed = geometry.ToReferenceSystem(SystemReferenceId.SridLambert2008);

            ApplyChange(new ParcelGeometryCrsWasChanged(
                ParcelId,
                CaPaKey,
                ExtendedWkbGeometry.Create(transformed)));
        }

        /// <summary>
        /// Reads a persisted geometry in the reference system its own bytes carry, falling back to Lambert 72
        /// for the SRID-less ones written before the event store wrote EWKB.
        /// </summary>
        /// <remarks>
        /// Qualified: <c>Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology</c> declares a
        /// <c>WKBReaderFactory</c> of its own, and the using directive for it in this file outranks
        /// <see cref="ParcelRegistry.WKBReaderFactory"/> from the enclosing namespace. GrAr's version throws on
        /// SRID-less bytes instead of falling back. See ADR 0005.
        /// </remarks>
        private static Geometry ReadGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            var extendedWkb = extendedWkbGeometry.ToByteArray();

            return ParcelRegistry.WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);
        }

        /// <summary>
        /// Guards the shape, and that the geometry is in one of the two reference systems this registry
        /// supports — not in a particular one of them.
        /// </summary>
        /// <remarks>
        /// Which of the two the event store holds is decided by <c>UseLambert2008EventStoreToggle</c> at the
        /// write boundary, where every incoming geometry is normalized to it. Pinning Lambert 72 here would
        /// reject everything the moment that toggle flips. See ADR 0005.
        /// </remarks>
        private static void GuardPolygon(Geometry? geometry)
        {
            if (geometry is Polygon
                && GeometryReferenceSystem.IsSupported(geometry.SRID)
                && GeometryValidator.IsValid(geometry))
            {
                return;
            }

            if (geometry is MultiPolygon multiPolygon
                && GeometryReferenceSystem.IsSupported(multiPolygon.SRID)
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
