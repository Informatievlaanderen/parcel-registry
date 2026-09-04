namespace ParcelRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;

    /// <summary>
    /// Moves a geometry between the two reference systems this registry supports, Lambert 72 (EPSG 31370)
    /// and Lambert 2008 (EPSG 3812). The single place that decides how that transformation is done, so a
    /// geometry the migrator transformed and the same geometry normalized on the way in come out
    /// byte-for-byte identical — otherwise the first GRB import after the conversion would report a
    /// geometry change for every parcel. See ADR 0005.
    /// </summary>
    /// <remarks>
    /// The transform output is not rounded. Address positions are rounded to centimetres because a position
    /// is a single point persisted at that precision; a parcel polygon is a boundary GRB delivers at full
    /// precision, and rounding its vertices would move the boundary rather than tidy it.
    /// </remarks>
    public static class GeometryReferenceSystem
    {
        public static bool IsSupported(int srid)
            => srid is SystemReferenceId.SridLambert72 or SystemReferenceId.SridLambert2008;

        /// <summary>
        /// The reference system a geometry's coordinates are actually in, decided by where they fall rather
        /// than by the SRID they carry.
        /// </summary>
        /// <remarks>
        /// The label cannot be trusted on the way in: <c>GrbXmlReader</c> reads GRB GML through a GMLReader
        /// built on the Lambert 72 geometry factory, so every polygon it produces carries SRID 31370 by
        /// construction — including, were GRB ever to deliver Lambert 2008, ones whose coordinates are
        /// nothing of the sort. <c>ConsumerAddressContext.FindAddressesWithinGeometry</c> decides the same
        /// way and for the same reason (ADR 0004). The two systems put Flanders ~500 km apart, so the
        /// envelopes cannot be confused.
        /// </remarks>
        public static int ReferenceSystemOfCoordinates(this Geometry geometry)
            => geometry.IsInsideFlandersUsingLambert08()
                ? SystemReferenceId.SridLambert2008
                : SystemReferenceId.SridLambert72;

        /// <summary>
        /// Puts a geometry in <paramref name="srid"/>, transforming it when its coordinates are in the other
        /// system and relabelling it when they are already in this one but the SRID says otherwise.
        /// </summary>
        /// <remarks>
        /// The explicit transform rather than <c>EnsureLambert08</c> / <c>EnsureLambert72</c>: those relabel
        /// whatever falls outside their envelope instead of transforming it, which for something that is
        /// about to be persisted would mean coordinates ~500 km from where the parcel is. See ADR 0005.
        /// </remarks>
        public static Geometry ToReferenceSystem(this Geometry geometry, int srid)
        {
            if (!IsSupported(srid))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(srid), srid, "Only Lambert 72 (31370) and Lambert 2008 (3812) are supported.");
            }

            if (geometry.ReferenceSystemOfCoordinates() == srid)
            {
                if (geometry.SRID == srid)
                {
                    return geometry;
                }

                var relabelled = geometry.Copy();
                relabelled.SRID = srid;
                return relabelled;
            }

            return srid == SystemReferenceId.SridLambert2008
                ? geometry.TransformFromLambert72To08()
                : geometry.TransformFromLambert08To72();
        }
    }
}
