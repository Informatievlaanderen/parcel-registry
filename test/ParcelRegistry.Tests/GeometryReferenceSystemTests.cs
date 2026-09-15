namespace ParcelRegistry.Tests
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    /// <summary>
    /// The one place that decides how a geometry moves between Lambert 72 and Lambert 2008, so that the
    /// migrator and the GRB importer cannot disagree about it. Kept out of the aggregate suite because the
    /// helper is shared with the write side, so its behaviour is not the aggregate's to assert. See ADR 0005.
    /// </summary>
    public class GeometryReferenceSystemTests
    {
        private static Polygon Lambert72Polygon => GeometryHelpers.ValidPolygon;

        /// <summary>
        /// Invalid in NTS, valid in SQL Server. GRB delivers polygons like this, which is why
        /// <c>FindAddressesWithinGeometry</c> fixes before it queries.
        /// </summary>
        private static Polygon InvalidPolygon => GeometryHelpers.InvalidNtsPolygon;

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, true)]
        [InlineData(SystemReferenceId.SridLambert2008, true)]
        [InlineData(0, false)]
        [InlineData(4326, false)]
        public void IsSupportedAnswersForTheLabelAlone(int srid, bool expected)
            => GeometryReferenceSystem.IsSupported(srid).Should().Be(expected);

        [Fact]
        public void AnUnsupportedTargetThrows()
        {
            var act = () => Lambert72Polygon.ToReferenceSystem(4326);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TheCoordinatesDecideTheSystem()
        {
            Lambert72Polygon.ReferenceSystemOfCoordinates()
                .Should().Be(SystemReferenceId.SridLambert72);

            Lambert72Polygon.ToReferenceSystem(SystemReferenceId.SridLambert2008)
                .ReferenceSystemOfCoordinates()
                .Should().Be(SystemReferenceId.SridLambert2008);
        }

        [Fact]
        public void TheRoundTripReturnsToWhereItStarted()
        {
            var roundTripped = Lambert72Polygon
                .ToReferenceSystem(SystemReferenceId.SridLambert2008)
                .ToReferenceSystem(SystemReferenceId.SridLambert72);

            roundTripped.SRID.Should().Be(SystemReferenceId.SridLambert72);
            roundTripped.Coordinate.X.Should().BeApproximately(Lambert72Polygon.Coordinate.X, 0.01);
            roundTripped.Coordinate.Y.Should().BeApproximately(Lambert72Polygon.Coordinate.Y, 0.01);
        }

        /// <summary>
        /// Asking for the system a geometry is already in relabels rather than transforms, so a mislabelled
        /// delivery is corrected instead of moved ~500 km.
        /// </summary>
        [Fact]
        public void CoordinatesAlreadyInTheTargetSystemAreRelabelledNotTransformed()
        {
            var mislabelled = (Polygon)Lambert72Polygon.Copy();
            mislabelled.SRID = SystemReferenceId.SridLambert2008;

            var result = mislabelled.ToReferenceSystem(SystemReferenceId.SridLambert72);

            result.SRID.Should().Be(SystemReferenceId.SridLambert72);
            result.Coordinate.X.Should().Be(Lambert72Polygon.Coordinate.X);
            result.Coordinate.Y.Should().Be(Lambert72Polygon.Coordinate.Y);
        }

        /// <summary>
        /// The reason the fix is on the transform branch only. A GRB delivery in the system the event store
        /// already holds has to pass through untouched, or the first import after this change would report a
        /// geometry change for every parcel whose polygon is invalid in NTS but valid in SQL Server.
        /// </summary>
        [Fact]
        public void AnInvalidGeometryAlreadyInTheTargetSystemIsNotFixed()
        {
            var invalid = InvalidPolygon;
            invalid.IsValid.Should().BeFalse();

            var result = invalid.ToReferenceSystem(SystemReferenceId.SridLambert72);

            result.Should().BeSameAs(invalid);
            result.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// And the reason the fix exists at all. <c>LambertTransformation</c> returns a geometry that is not
        /// <c>IsValid</c> untouched, so without fixing first this would come back carrying SRID 3812 with its
        /// Lambert 72 coordinates unmoved. See ADR 0004.
        /// </summary>
        [Fact]
        public void AnInvalidGeometryIsFixedBeforeItIsTransformed()
        {
            var invalid = InvalidPolygon;
            invalid.IsValid.Should().BeFalse();

            var transformed = invalid.ToReferenceSystem(SystemReferenceId.SridLambert2008);

            transformed.IsValid.Should().BeTrue();
            transformed.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            transformed.ReferenceSystemOfCoordinates().Should().Be(SystemReferenceId.SridLambert2008);

            // Actually moved, rather than relabelled in place.
            transformed.Coordinate.X.Should().BeGreaterThan(500000);
        }

        /// <summary>
        /// Fixing repairs the polygon without reshaping it: the transformed area matches the area of the same
        /// polygon fixed on its own, to within the scale difference between the two projections.
        /// </summary>
        [Fact]
        public void FixingDoesNotChangeWhereTheParcelIs()
        {
            var invalid = InvalidPolygon;

            var fixedOnly = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(invalid);
            var transformed = invalid.ToReferenceSystem(SystemReferenceId.SridLambert2008);

            // Lambert 72 and Lambert 2008 are different projections of the same place, so the same parcel does
            // not have exactly the same area in both — over Flanders they sit about 5 ppm apart, which for
            // this ~12 166 m² parcel is some 6 cm². A fix that reshaped the polygon rather than repairing its
            // self-intersection would move the area by orders of magnitude more than that.
            var relativeDifference = Math.Abs(transformed.Area - fixedOnly.Area) / fixedOnly.Area;

            relativeDifference.Should().BeLessThan(1e-4);
        }

        /// <summary>
        /// The caller's geometry is never mutated, whichever branch is taken — the migrator reads a stream's
        /// geometry and would otherwise be handing a modified instance to the event it appends.
        /// </summary>
        [Fact]
        public void TheCallersGeometryIsLeftAlone()
        {
            var original = Lambert72Polygon;
            var originalX = original.Coordinate.X;

            original.ToReferenceSystem(SystemReferenceId.SridLambert2008);

            original.SRID.Should().Be(SystemReferenceId.SridLambert72);
            original.Coordinate.X.Should().Be(originalX);
        }
    }
}
