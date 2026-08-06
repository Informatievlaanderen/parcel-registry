namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Parcel;
    using Parcel.Events;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.ParcelVersion;
    using Xunit;

    /// <summary>
    /// ParcelVersion is a history table: it appends a row per event and copies the previous row's geometry
    /// forward, so version rows written before the conversion keep SRID 31370 permanently. That is the
    /// intended end state, not a transient window — the table records what the register actually held, and
    /// it genuinely held Lambert 72 until the conversion event and Lambert 2008 after it.
    ///
    /// This test exists so that a future rebuild-to-normalize is a deliberate decision rather than an
    /// accident. See ADR 0003.
    /// </summary>
    public class GivenVersionHistorySpansBothReferenceSystems : IntegrationProjectionTest<ParcelVersionProjections>
    {
        private const string Namespace = "https://data.vlaanderen.be/id/perceel";
        private readonly Mock<IAddressRepository> _addressRepository = new();

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

        [Fact]
        public async Task ThenEarlierVersionsStayLambert72AndLaterOnesAreLambert2008()
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

            var position = fixture.Create<long>();

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, fixture.Create<string>() },
                        { Envelope.PositionMetadataKey, position },
                        { Envelope.EventNameMetadataKey, nameof(ParcelWasImported) }
                    })),
                    new Envelope<ParcelGeometryWasChanged>(new Envelope(geometryWasChanged, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, fixture.Create<string>() },
                        { Envelope.PositionMetadataKey, position + 1 },
                        { Envelope.EventNameMetadataKey, nameof(ParcelGeometryWasChanged) }
                    })))
                .Then(async context =>
                {
                    var versions = await Task.FromResult(context.ParcelVersions
                        .Where(x => x.ParcelId == geometryWasChanged.ParcelId)
                        .OrderBy(x => x.Position)
                        .ToList());

                    versions.Should().HaveCount(2);

                    versions[0].Geometry!.SRID.Should().Be(SystemReferenceId.SridLambert72);
                    versions[0].Geometry!.Coordinates.First().X.Should().BeApproximately(140284.15, 0.01);

                    versions[1].Geometry!.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                    versions[1].Geometry!.Coordinates.First().X.Should().BeApproximately(640281.95, 0.01);
                });
        }

        protected override ParcelVersionProjections CreateProjection()
            => new(
                _addressRepository.Object,
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
