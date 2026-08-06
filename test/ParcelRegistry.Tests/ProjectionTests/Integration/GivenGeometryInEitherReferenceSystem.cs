namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Parcel;
    using Parcel.Events;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.ParcelLatestItemV2;
    using Xunit;

    /// <summary>
    /// The PostGIS geometry column carries whichever reference system the event store wrote, so rows
    /// self-describe and ST_SRID can be branched on. The column is deliberately allowed to hold both while
    /// the conversion runs. See ADR 0003.
    /// </summary>
    public class GivenGeometryInEitherReferenceSystem : IntegrationProjectionTest<ParcelLatestItemV2Projections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/perceel";

        [Fact]
        public async Task WhenPersistedInLambert72_ThenGeometryIsLambert72()
            => await AssertMigratedSrid(new WithExtendedWkbGeometryPolygon(), SystemReferenceId.SridLambert72, 140284.15);

        [Fact]
        public async Task WhenPersistedInLambert2008_ThenGeometryIsLambert2008()
            => await AssertMigratedSrid(new WithExtendedWkbGeometryPolygonLambert2008(), SystemReferenceId.SridLambert2008, 640281.95);

        /// <summary>
        /// The regression this suite exists for: GrAr.Common's CreateForEwkb throws on SRID-less bytes, so a
        /// naive swap to it would break every geometry persisted before the event store wrote EWKB.
        /// </summary>
        [Fact]
        public async Task WhenPersistedWithoutSrid_ThenGeometryIsLambert72()
            => await AssertMigratedSrid(new WithExtendedWkbGeometryPolygonWithoutSrid(), SystemReferenceId.SridLambert72, 140284.15);

        [Fact]
        public async Task WhenGeometryWasChangedToLambert2008_ThenGeometryIsLambert2008()
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

            await Sut.Given(parcelWasImported, geometryWasChanged)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(geometryWasChanged.ParcelId);

                    latestItem.Should().NotBeNull();
                    latestItem!.Geometry.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                    latestItem.Geometry.Coordinates.First().X.Should().BeApproximately(640281.95, 0.01);
                });
        }

        private static Fixture CreateFixture(ICustomization geometry)
        {
            var fixture = new Fixture();
            fixture.Customize(new InfrastructureCustomization());
            fixture.Customize(new WithFixedParcelId());
            fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            fixture.Customize(new WithParcelStatus());
            fixture.Customize(geometry);

            return fixture;
        }

        private async Task AssertMigratedSrid(ICustomization geometry, int expectedSrid, double expectedFirstX)
        {
            var message = CreateFixture(geometry).Create<ParcelWasMigrated>();

            await Sut.Given(message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);

                    latestItem.Should().NotBeNull();
                    latestItem!.Geometry.SRID.Should().Be(expectedSrid);
                    latestItem.Geometry.Coordinates.First().X.Should().BeApproximately(expectedFirstX, 0.01);
                });
        }

        protected override ParcelLatestItemV2Projections CreateProjection()
            => new(new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
