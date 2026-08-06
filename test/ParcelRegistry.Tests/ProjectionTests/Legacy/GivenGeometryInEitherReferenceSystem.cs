namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using Parcel.Events;
    using Projections.Legacy.ParcelDetail;
    using Xunit;

    /// <summary>
    /// The Gml column follows whatever the event store writes: it is not pinned to Lambert 72, and
    /// ConvertToGml labels it with the srsName the geometry was persisted in, so a mixed window is legible
    /// rather than ambiguous. See ADR 0003.
    /// </summary>
    public class GivenGeometryInEitherReferenceSystem : ParcelLegacyProjectionTest<ParcelDetailProjections>
    {
        private const string SrsNameLambert72 = "srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\"";
        private const string SrsNameLambert2008 = "srsName=\"https://www.opengis.net/def/crs/EPSG/0/3812\"";

        // First vertex of GeometryHelpers.ValidGmlPolygon and of its genuine Lambert 2008 transform.
        // Asserted against fixed coordinates rather than against the event, so a reader that relabelled the
        // reference system without moving the coordinates — or vice versa — fails here.
        private const double FirstXLambert72 = 140284.15;
        private const double FirstXLambert2008 = 640281.95;

        private static Fixture CreateFixture(ICustomization geometry)
        {
            var fixture = new Fixture();
            fixture.Customize(new InfrastructureCustomization());
            fixture.Customize(new WithParcelStatus());
            fixture.Customize(new WithFixedParcelId());
            fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            fixture.Customize(geometry);

            return fixture;
        }

        private static void AssertGml(string? gml, string expectedSrsName, double expectedFirstX)
        {
            gml.Should().NotBeNull();
            gml.Should().Contain(expectedSrsName);

            var geometry = GeometryHelpers.CreateGmlReader().Read(gml);
            geometry.Coordinates.First().X.Should().BeApproximately(expectedFirstX, 0.01);
        }

        private async Task AssertMigratedGml(ICustomization geometry, string expectedSrsName, double expectedFirstX)
        {
            var message = CreateFixture(geometry).Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() }
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(message, metadata)))
                .Then(async context =>
                {
                    var parcelDetail = await context.ParcelDetails.FindAsync(message.ParcelId);

                    parcelDetail.Should().NotBeNull();
                    AssertGml(parcelDetail!.Gml, expectedSrsName, expectedFirstX);
                    parcelDetail.GmlType.Should().Be("Polygon");
                });
        }

        [Fact]
        public async Task WhenPersistedInLambert72_ThenGmlIsLambert72()
            => await AssertMigratedGml(new WithExtendedWkbGeometryPolygon(), SrsNameLambert72, FirstXLambert72);

        [Fact]
        public async Task WhenPersistedInLambert2008_ThenGmlIsLambert2008()
            => await AssertMigratedGml(new WithExtendedWkbGeometryPolygonLambert2008(), SrsNameLambert2008, FirstXLambert2008);

        /// <summary>
        /// The regression this suite exists for: GrAr.Common's CreateForEwkb throws on SRID-less bytes, so a
        /// naive swap to it would break every geometry persisted before the event store wrote EWKB.
        /// </summary>
        [Fact]
        public async Task WhenPersistedWithoutSrid_ThenGmlIsLambert72()
            => await AssertMigratedGml(new WithExtendedWkbGeometryPolygonWithoutSrid(), SrsNameLambert72, FirstXLambert72);

        [Fact]
        public async Task WhenGeometryWasChangedToLambert2008_ThenGmlIsLambert2008()
        {
            var fixture = CreateFixture(new WithExtendedWkbGeometryPolygon());

            var parcelWasImported = fixture.Create<ParcelWasImported>();

            // Built by hand rather than from a second fixture: the parcel id is fixed per fixture instance,
            // so two fixtures would describe two different parcels.
            var geometryWasChanged = new ParcelGeometryWasChanged(
                new ParcelId(parcelWasImported.ParcelId),
                new VbrCaPaKey(parcelWasImported.CaPaKey),
                GeometryHelpers.ValidGmlPolygonLambert2008.ToExtendedWkbGeometryLambert2008());
            geometryWasChanged.SetFixtureProvenance(fixture);

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, parcelWasImported.GetHash() }
                    })),
                    new Envelope<ParcelGeometryWasChanged>(new Envelope(geometryWasChanged, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, geometryWasChanged.GetHash() }
                    })))
                .Then(async context =>
                {
                    var parcelDetail = await context.ParcelDetails.FindAsync(geometryWasChanged.ParcelId);

                    parcelDetail.Should().NotBeNull();
                    AssertGml(parcelDetail!.Gml, SrsNameLambert2008, FirstXLambert2008);
                });
        }

        protected override ParcelDetailProjections CreateProjection() => new();
    }
}
