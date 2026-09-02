namespace ParcelRegistry.Tests.AggregateTests.WhenTransformingToLambert2008
{
    using System.Linq;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        private static ExtendedWkbGeometry Lambert72Geometry
            => GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry();

        /// <summary>
        /// <see cref="Lambert72Geometry"/> as the shared transformation produces it. Parcel geometries are not
        /// rounded, so this cannot be a GML constant of its own: it would differ from the aggregate's output in
        /// the last decimals. What that output actually *is* is pinned against fixed coordinates by
        /// <see cref="TheTransformedGeometryIsTheSameParcelInLambert2008"/>; the cases below then assert the
        /// aggregate produces exactly it, for whichever parcel they are about.
        /// </summary>
        private static ExtendedWkbGeometry Lambert2008Geometry
            => ExtendedWkbGeometry.Create(
                GeometryHelpers.ValidPolygon.ToReferenceSystem(SystemReferenceId.SridLambert2008));

        [Fact]
        public void TheTransformedGeometryIsTheSameParcelInLambert2008()
        {
            var transformed = _wkbReader.Read(Lambert2008Geometry);

            transformed.SRID.Should().Be(SystemReferenceId.SridLambert2008);

            // The first vertex of GeometryHelpers.ValidGmlPolygonLambert2008, the genuine Lambert 2008
            // counterpart of GeometryHelpers.ValidGmlPolygon, to the centimetre it is documented at.
            transformed.Coordinates.First().X.Should().BeApproximately(640281.95, 0.01);
            transformed.Coordinates.First().Y.Should().BeApproximately(686723.97, 0.01);
        }

        [Fact]
        public void ThenParcelGeometryCrsWasChanged()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(Lambert72Geometry)
                .Build();

            var command = new TransformToLambert2008Builder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported)
                .When(command)
                .Then(new ParcelStreamId(parcelId),
                    new ParcelGeometryCrsWasChanged(parcelId, caPaKey, Lambert2008Geometry)));
        }

        /// <summary>
        /// Geometries written before the event store wrote EWKB carry no SRID at all. They are Lambert 72 by
        /// definition (see ADR 0003), so they transform like any other, and come out carrying SRID 3812.
        /// </summary>
        [Fact]
        public void WithGeometryWithoutSrid_ThenGeometryIsTransformedFromLambert72()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometryWithoutSrid())
                .Build();

            var command = new TransformToLambert2008Builder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported)
                .When(command)
                .Then(new ParcelStreamId(parcelId),
                    new ParcelGeometryCrsWasChanged(parcelId, caPaKey, Lambert2008Geometry)));
        }

        /// <summary>Re-running the transformation over a stream must be a no-op, not a second transform.</summary>
        [Fact]
        public void WithGeometryAlreadyInLambert2008_ThenNone()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(Lambert2008Geometry)
                .Build();

            var command = new TransformToLambert2008Builder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported)
                .When(command)
                .ThenNone());
        }

        /// <summary>
        /// The transformation is not an edit, so unlike changing the geometry it must reach removed parcels
        /// too — leaving them behind would keep the event store mixed forever.
        /// </summary>
        [Fact]
        public void WithRemovedParcel_ThenParcelGeometryCrsWasChanged()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithIsRemoved()
                .WithExtendedWkbGeometry(Lambert72Geometry)
                .Build();

            var command = new TransformToLambert2008Builder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasMigrated)
                .When(command)
                .Then(new ParcelStreamId(parcelId),
                    new ParcelGeometryCrsWasChanged(parcelId, caPaKey, Lambert2008Geometry)));
        }

        /// <summary>A retired parcel holds a geometry like any other and must be transformed too.</summary>
        [Theory]
        [InlineData("Realized")]
        [InlineData("Retired")]
        public void WithAnyParcelStatus_ThenParcelGeometryCrsWasChanged(string status)
        {
            var parcelStatus = ParcelStatus.Parse(status);

            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithStatus(parcelStatus)
                .WithExtendedWkbGeometry(Lambert72Geometry)
                .Build();

            var command = new TransformToLambert2008Builder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasMigrated)
                .When(command)
                .Then(new ParcelStreamId(parcelId),
                    new ParcelGeometryCrsWasChanged(parcelId, caPaKey, Lambert2008Geometry)));
        }

        /// <summary>
        /// The attached addresses are untouched: the parcel does not move, so nothing about which addresses
        /// fall inside it changes.
        /// </summary>
        [Fact]
        public void WithAttachedAddresses_ThenOnlyTheGeometryCrsWasChanged()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(Lambert72Geometry)
                .Build();

            var parcelAddressWasAttached = new ParcelAddressWasAttachedV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithAddress(new AddressPersistentLocalId(1))
                .Build();

            var command = new TransformToLambert2008Builder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported, parcelAddressWasAttached)
                .When(command)
                .Then(new ParcelStreamId(parcelId),
                    new ParcelGeometryCrsWasChanged(parcelId, caPaKey, Lambert2008Geometry)));
        }

        [Fact]
        public void StateCheck()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(Lambert72Geometry)
                .Build();

            var parcelGeometryCrsWasChanged = new ParcelGeometryCrsWasChangedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithVbrCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(Lambert2008Geometry)
                .Build();

            var parcel = new ParcelFactory(NoSnapshotStrategy.Instance, new Mock<IAddresses>().Object).Create();
            parcel.Initialize(new object[]
            {
                parcelWasImported,
                parcelGeometryCrsWasChanged
            });

            parcel.Geometry.Should().Be(Lambert2008Geometry);

            parcel.Geometry.ToString().ToByteArray().TryReadSrid(out var srid).Should().BeTrue();
            srid.Should().Be(SystemReferenceId.SridLambert2008);
        }
    }
}
