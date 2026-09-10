namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using NetTopologySuite.Geometries;
    using Parcel;
    using Parcel.Commands;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Whatever GRB delivers is normalized to the reference system the event store holds, so the aggregate
    /// only ever sees one. The two directions are not symmetric in likelihood — GRB delivers Lambert 72, so
    /// the transform to Lambert 2008 is the real path and the one back is a safeguard against a delivery
    /// that should not happen — but both are here so neither can rot. See ADR 0005.
    /// </summary>
    public class GivenEventStoreInEitherReferenceSystem : ParcelRegistryTest
    {
        public GivenEventStoreInEitherReferenceSystem(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        { }

        // GeometryHelpers.ValidGmlPolygon and its genuine Lambert 2008 transform, so a test asserting one
        // against the other is asserting the same physical parcel rather than restating the transform.
        private const double FirstXLambert72 = 140284.15;
        private const double FirstXLambert2008 = 640281.95;

        private static Polygon Lambert72Polygon => GeometryHelpers.ValidPolygon;

        private Polygon Lambert2008Polygon =>
            (Polygon)_wkbReader.Read(GeometryHelpers.ValidGmlPolygonLambert2008.ToExtendedWkbGeometryLambert2008());

        [Fact]
        public async Task WhenEventStoreIsLambert72AndGrbDeliversLambert72_ThenGeometryIsPersistedUnchanged()
            => await AssertImportedGeometry(
                useLambert2008EventStore: false,
                grbGeometry: Lambert72Polygon,
                expectedSrid: SystemReferenceId.SridLambert72,
                expectedFirstX: FirstXLambert72);

        [Fact]
        public async Task WhenEventStoreIsLambert2008AndGrbDeliversLambert72_ThenGeometryIsTransformed()
            => await AssertImportedGeometry(
                useLambert2008EventStore: true,
                grbGeometry: Lambert72Polygon,
                expectedSrid: SystemReferenceId.SridLambert2008,
                expectedFirstX: FirstXLambert2008);

        [Fact]
        public async Task WhenEventStoreIsLambert2008AndGrbDeliversLambert2008_ThenGeometryIsPersistedUnchanged()
            => await AssertImportedGeometry(
                useLambert2008EventStore: true,
                grbGeometry: Lambert2008Polygon,
                expectedSrid: SystemReferenceId.SridLambert2008,
                expectedFirstX: FirstXLambert2008);

        /// <summary>
        /// The safeguard. It should not occur — GRB delivers Lambert 72 — but a delivery in the other system
        /// while the event store is still Lambert 72 would otherwise persist coordinates ~500 km from where
        /// the parcel is.
        /// </summary>
        [Fact]
        public async Task WhenEventStoreIsLambert72AndGrbDeliversLambert2008_ThenGeometryIsTransformedBack()
            => await AssertImportedGeometry(
                useLambert2008EventStore: false,
                grbGeometry: Lambert2008Polygon,
                expectedSrid: SystemReferenceId.SridLambert72,
                expectedFirstX: FirstXLambert72);

        /// <summary>
        /// The realistic shape of a wrong delivery: GrbXmlReader reads GRB GML through a GMLReader built on
        /// the Lambert 72 geometry factory, so a Lambert 2008 polygon arrives carrying SRID 31370. The
        /// normalization decides on the coordinates, not the label, which is the only way to catch this.
        /// </summary>
        [Fact]
        public async Task WhenGrbDeliversLambert2008LabelledLambert72_ThenTheCoordinatesDecide()
        {
            var mislabelled = (Polygon)Lambert2008Polygon.Copy();
            mislabelled.SRID = SystemReferenceId.SridLambert72;

            await AssertImportedGeometry(
                useLambert2008EventStore: true,
                grbGeometry: mislabelled,
                expectedSrid: SystemReferenceId.SridLambert2008,
                expectedFirstX: FirstXLambert2008);
        }

        /// <summary>
        /// The same normalization on the other write path, and the one that runs for parcels that already
        /// exist — which after the conversion is all of them.
        /// </summary>
        [Fact]
        public async Task WhenChangingGeometryAndEventStoreIsLambert2008_ThenGeometryIsTransformed()
        {
            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());
            var parcelId = ParcelId.CreateFor(new VbrCaPaKey(caPaKey.VbrCaPaKey));

            DispatchArrangeCommand(
                new ImportParcel(
                    new VbrCaPaKey(caPaKey.VbrCaPaKey),
                    ExtendedWkbGeometry.CreateEWkb(GeometryHelpers.ValidPolygon3)!,
                    new List<AddressPersistentLocalId>(),
                    Fixture.Create<Provenance>()));

            var sut = new ChangeParcelGeometryHandler(
                Container,
                new FakeConsumerAddressContextFactory().CreateDbContext(),
                new UseLambert2008EventStoreToggle(true));

            await sut.Handle(
                new ChangeParcelGeometryRequest(new GrbParcel(caPaKey, Lambert72Polygon, 9, DateTime.Now)),
                CancellationToken.None);

            var parcel = await Container.Resolve<IParcels>().GetAsync(new ParcelStreamId(parcelId));

            var persisted = _wkbReader.Read(parcel.Geometry);
            persisted.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            persisted.Coordinates.First().X.Should().BeApproximately(FirstXLambert2008, 0.01);
        }

        private async Task AssertImportedGeometry(
            bool useLambert2008EventStore,
            Polygon grbGeometry,
            int expectedSrid,
            double expectedFirstX)
        {
            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());
            var parcelId = ParcelId.CreateFor(new VbrCaPaKey(caPaKey.VbrCaPaKey));

            var sut = new ImportParcelHandler(
                Container,
                new FakeConsumerAddressContextFactory().CreateDbContext(),
                new UseLambert2008EventStoreToggle(useLambert2008EventStore));

            await sut.Handle(
                new ImportParcelRequest(new GrbParcel(caPaKey, grbGeometry, 9, DateTime.Now)),
                CancellationToken.None);

            var parcel = await Container.Resolve<IParcels>().GetAsync(new ParcelStreamId(parcelId));

            var persisted = _wkbReader.Read(parcel.Geometry);
            persisted.SRID.Should().Be(expectedSrid);
            persisted.Coordinates.First().X.Should().BeApproximately(expectedFirstX, 0.01);
        }
    }
}
