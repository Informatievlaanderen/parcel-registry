namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Linq;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Consumer.Address;
    using FluentAssertions;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Parcels are converted to Lambert 2008 after every address already is, so the polygon arriving here is
    /// Lambert 72 before the conversion and Lambert 2008 after it, and has to find the same addresses either
    /// way. See ADR 0004.
    /// </summary>
    public class GivenParcelGeometryInEitherReferenceSystem : ParcelRegistryTest
    {
        public GivenParcelGeometryInEitherReferenceSystem(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        { }

        [Fact]
        public void WhenParcelIsLambert2008_ThenAddressesAreMatchedOnTheLambert2008Column()
        {
            var context = new FakeConsumerAddressContextFactory().CreateDbContext();

            var within = Fixture.Create<AddressPersistentLocalId>();
            context.AddressConsumerItems.Add(new AddressConsumerItem(
                within,
                AddressStatus.Current,
                "geometryMethod",
                "geometrySpec",
                GeometryHelpers.ValidPoint1InPolgyon2,
                GeometryHelpers.ValidPoint1InPolygon2Lambert2008));

            var outside = Fixture.Create<AddressPersistentLocalId>();
            context.AddressConsumerItems.Add(new AddressConsumerItem(
                outside,
                AddressStatus.Current,
                "geometryMethod",
                "geometrySpec",
                GeometryHelpers.PointOutsideOfValidPolygon2,
                GeometryHelpers.PointOutsideOfValidPolygon2Lambert2008));

            context.SaveChanges();

            var result = context
                .FindAddressesWithinGeometry(GeometryHelpers.ValidPolygon2Lambert2008)
                .ToList();

            result.Should().ContainSingle();
            result.Single().AddressPersistentLocalId.Should().Be((int)within);
        }

        /// <summary>
        /// The polygon's SRID is not what decides this. GrbXmlReader reads GRB GML through a GMLReader built
        /// on the Lambert 72 geometry factory, so a Lambert 2008 polygon arrives labelled 31370; dispatching
        /// on the SRID would query the Lambert 72 column with Lambert 2008 coordinates and match nothing.
        /// </summary>
        [Fact]
        public void WhenParcelIsLambert2008ButLabelledLambert72_ThenTheCoordinatesStillDecide()
        {
            var context = new FakeConsumerAddressContextFactory().CreateDbContext();

            var within = Fixture.Create<AddressPersistentLocalId>();
            context.AddressConsumerItems.Add(new AddressConsumerItem(
                within,
                AddressStatus.Current,
                "geometryMethod",
                "geometrySpec",
                GeometryHelpers.ValidPoint1InPolgyon2,
                GeometryHelpers.ValidPoint1InPolygon2Lambert2008));
            context.SaveChanges();

            var mislabelled = GeometryHelpers.ValidPolygon2Lambert2008;
            mislabelled.SRID = SystemReferenceId.SridLambert72;

            var result = context.FindAddressesWithinGeometry(mislabelled).ToList();

            result.Should().ContainSingle();
            result.Single().AddressPersistentLocalId.Should().Be((int)within);
        }

        /// <summary>
        /// The conversion order makes this unreachable in production. The guard is what turns that
        /// assumption about another register's conversion into a stopped importer rather than parcels
        /// quietly importing without their addresses.
        /// </summary>
        [Fact]
        public void WhenAnAddressHasNoLambert2008Position_ThenALambert2008ParcelIsRefused()
        {
            var context = new FakeConsumerAddressContextFactory().CreateDbContext();

            context.AddressConsumerItems.Add(new AddressConsumerItem(
                Fixture.Create<AddressPersistentLocalId>(),
                AddressStatus.Current,
                "geometryMethod",
                "geometrySpec",
                GeometryHelpers.ValidPoint1InPolgyon2));
            context.SaveChanges();

            var act = () => context.FindAddressesWithinGeometry(GeometryHelpers.ValidPolygon2Lambert2008).ToList();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*no Lambert 2008 position*");
        }

        /// <summary>
        /// A Lambert 72 parcel is unaffected by the Lambert 2008 column being empty — that is what lets the
        /// importer keep running from the moment this column is added until parcels are converted.
        /// </summary>
        [Fact]
        public void WhenParcelIsLambert72_ThenAnEmptyLambert2008ColumnDoesNotMatter()
        {
            var context = new FakeConsumerAddressContextFactory().CreateDbContext();

            var within = Fixture.Create<AddressPersistentLocalId>();
            context.AddressConsumerItems.Add(new AddressConsumerItem(
                within,
                AddressStatus.Current,
                "geometryMethod",
                "geometrySpec",
                GeometryHelpers.ValidPoint1InPolgyon2));
            context.SaveChanges();

            var result = context.FindAddressesWithinGeometry(GeometryHelpers.ValidPolygon2).ToList();

            result.Should().ContainSingle();
            result.Single().AddressPersistentLocalId.Should().Be((int)within);
        }
    }
}
