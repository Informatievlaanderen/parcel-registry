namespace ParcelRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Parcel;
    using ParcelRegistry.Consumer.Address;
    using ParcelRegistry.Consumer.Address.Projections;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// The address event store is converted from Lambert 72 to Lambert 2008 independently of the parcel one,
    /// so this consumer has to read positions in whichever system the EWKB carries and hold both. See ADR 0004.
    /// </summary>
    public sealed class GivenAddressPositionInEitherReferenceSystem
        : KafkaProjectionTest<ConsumerAddressContext, BackOfficeKafkaProjection>
    {
        /// <summary>
        /// A transformed position is rounded to centimetres, so the two columns agree to within that rather
        /// than exactly.
        /// </summary>
        private const double ToleranceInMeters = 0.02;

        /// <summary>
        /// A Lambert 72 position carrying more precision than the centimetre a transformed position is
        /// rounded to. Positions like this are real — the event store holds values such as
        /// 198794.27000000083 — and they are what makes "stored verbatim" mean something: a value that had
        /// been through a transform and back would have lost these digits.
        /// </summary>
        private const string HighPrecisionLambert72Point = "POINT (140000.123456 186000.654321)";

        public GivenAddressPositionInEitherReferenceSystem(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public async Task WhenPersistedInLambert72_ThenItIsStoredVerbatimAndLambert2008IsDerived()
        {
            var proposed = CreateAddressWasProposedV2(GeometryHelpers.CreateFromWkt(HighPrecisionLambert72Point));
            var persisted = ReadPosition(proposed.ExtendedWkbGeometry);

            Given(proposed);

            await Then(async context =>
            {
                var address = await context.AddressConsumerItems.FindAsync(proposed.AddressPersistentLocalId);

                address!.Position.SRID.Should().Be(SystemReferenceId.SridLambert72);
                address.Position.X.Should().Be(persisted.X);
                address.Position.Y.Should().Be(persisted.Y);

                address.PositionLambert2008.Should().NotBeNull();
                address.PositionLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                address.PositionLambert2008.Distance(persisted.EnsureLambert08()).Should().BeLessThan(ToleranceInMeters);
            });
        }

        [Fact]
        public async Task WhenPersistedInLambert2008_ThenItIsStoredVerbatimAndLambert72IsDerived()
        {
            Fixture.Customize(new WithExtendedWkbGeometryPointLambert2008());

            var proposed = CreateAddressWasProposedV2();
            var persisted = ReadPosition(proposed.ExtendedWkbGeometry);

            Given(proposed);

            await Then(async context =>
            {
                var address = await context.AddressConsumerItems.FindAsync(proposed.AddressPersistentLocalId);

                address!.PositionLambert2008.Should().NotBeNull();
                address.PositionLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                address.PositionLambert2008.X.Should().Be(persisted.X);
                address.PositionLambert2008.Y.Should().Be(persisted.Y);

                address.Position.SRID.Should().Be(SystemReferenceId.SridLambert72);
                address.Position.Distance(persisted.EnsureLambert72()).Should().BeLessThan(ToleranceInMeters);
            });
        }

        /// <summary>
        /// Positions persisted before the address event store wrote EWKB carry no SRID. GrAr's
        /// <c>CreateForEwkb</c> throws on those; reading through <see cref="ParcelRegistry.WKBReaderFactory"/>
        /// is what makes them Lambert 72. Removing that fallback makes this test throw rather than compile to
        /// something subtly different.
        /// </summary>
        [Fact]
        public async Task WhenPersistedWithoutSrid_ThenItIsReadAsLambert72()
        {
            var withoutSrid = GeometryHelpers.CreateWkbPointWithoutSrid(
                GeometryHelpers.Lambert72PointX,
                GeometryHelpers.Lambert72PointY);

            var proposed = CreateAddressWasProposedV2(withoutSrid);

            Given(proposed);

            await Then(async context =>
            {
                var address = await context.AddressConsumerItems.FindAsync(proposed.AddressPersistentLocalId);

                address!.Position.SRID.Should().Be(SystemReferenceId.SridLambert72);
                address.Position.X.Should().Be(GeometryHelpers.Lambert72PointX);
                address.Position.Y.Should().Be(GeometryHelpers.Lambert72PointY);

                address.PositionLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            });
        }

        /// <summary>
        /// The conversion re-expresses a position, it does not move it, so the Lambert 72 column keeps its
        /// exact as-published value. It is queried until parcels are converted, and
        /// <c>FindAddressesWithinGeometry</c> matches on Touches as well as Contains, so rewriting every
        /// address in the register with a centimetre-rounded round trip of itself is not free.
        /// </summary>
        [Fact]
        public async Task WhenCrsWasChanged_ThenLambert72PositionIsLeftUntouched()
        {
            var proposed = CreateAddressWasProposedV2(GeometryHelpers.CreateFromWkt(HighPrecisionLambert72Point));
            var persisted = ReadPosition(proposed.ExtendedWkbGeometry);

            var inLambert2008 = ReadPosition(proposed.ExtendedWkbGeometry).EnsureLambert08(2);
            var converted = GeometryHelpers.CreateEwkbPointLambert2008(inLambert2008.X, inLambert2008.Y);

            var crsWasChanged = new AddressPositionCrsWasChanged(
                proposed.StreetNamePersistentLocalId,
                proposed.AddressPersistentLocalId,
                proposed.GeometryMethod,
                proposed.GeometrySpecification,
                converted.ToString(),
                Fixture.Create<Provenance>());

            Given(proposed, crsWasChanged);

            await Then(async context =>
            {
                var address = await context.AddressConsumerItems.FindAsync(proposed.AddressPersistentLocalId);

                address!.Position.X.Should().Be(persisted.X);
                address.Position.Y.Should().Be(persisted.Y);

                address.PositionLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                address.PositionLambert2008.Distance(persisted.EnsureLambert08()).Should().BeLessThan(ToleranceInMeters);
            });
        }

        /// <summary>
        /// A position outside both envelopes is not transformed at all — it would just have an SRID stamped
        /// on unmoved coordinates, landing ~500 km away, inside no parcel and silently attached to none.
        /// </summary>
        [Fact]
        public async Task WhenPositionLiesOutsideFlanders_ThenItIsRefused()
        {
            var outsideFlanders = GeometryHelpers.CreateFromWkt("POINT (1 1)");

            var proposed = CreateAddressWasProposedV2(outsideFlanders);

            Given(proposed);

            var act = () => Then(_ => Task.CompletedTask);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*outside Flanders*");
        }

        private static NetTopologySuite.Geometries.Point ReadPosition(string extendedWkbGeometry)
        {
            var bytes = extendedWkbGeometry.ToByteArray()!;

            return (NetTopologySuite.Geometries.Point)WKBReaderFactory.CreateForEwkb(bytes).Read(bytes);
        }

        private AddressWasProposedV2 CreateAddressWasProposedV2(ExtendedWkbGeometry? position = null)
            => Fixture
                .Build<AddressWasProposedV2>()
                .FromFactory(() => new AddressWasProposedV2(
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    (position ?? Fixture.Create<ExtendedWkbGeometry>()).ToString(),
                    Fixture.Create<Provenance>()))
                .Create();

        protected override ConsumerAddressContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerAddressContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerAddressContext(options);
        }

        protected override BackOfficeKafkaProjection CreateProjection() => new BackOfficeKafkaProjection();
    }
}
