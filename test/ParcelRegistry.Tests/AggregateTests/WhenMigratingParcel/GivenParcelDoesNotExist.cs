namespace ParcelRegistry.Tests.AggregateTests.WhenMigratingParcel
{
    using Api.BackOffice.Abstractions.Extensions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Parcel;
    using Parcel.Events;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using ParcelStatus = ParcelRegistry.Legacy.ParcelStatus;

    public class GivenParcelDoesNotExist : ParcelRegistryTest
    {
        public GivenParcelDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenParcelWasMigratedEvent()
        {
            var command = new MigrateParcelBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.NewParcelId),
                    new ParcelWasMigrated(
                        command.OldParcelId,
                        command.NewParcelId,
                        command.CaPaKey,
                        command.ParcelStatus,
                        command.IsRemoved,
                        command.AddressPersistentLocalIds,
                        command.ExtendedWkbGeometry))));
        }

        [Fact]
        public void WithValidMultiPolygon_ThenParcelWasMigratedEvent()
        {
            var command = new MigrateParcelBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.NewParcelId),
                    new ParcelWasMigrated(
                        command.OldParcelId,
                        command.NewParcelId,
                        command.CaPaKey,
                        command.ParcelStatus,
                        command.IsRemoved,
                        command.AddressPersistentLocalIds,
                        command.ExtendedWkbGeometry))));
        }

        [Fact]
        public void WithInvalidPolygon_ThenThrowsPolygonIsInvalidException()
        {
            var command = new MigrateParcelBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.GmlToExtendedWkbGeometry())
                .Build();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void WithInValidMultiPolygon_ThenParcelWasMigratedEvent()
        {
            var command = new MigrateParcelBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithExtendedWkbGeometry(GeometryHelpers.ToExtendedWkbGeometry(new MultiPolygon(new[] { GeometryHelpers.InValidPolygon })))
                .Build();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void StateCheck()
        {
            var command = new MigrateParcelBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            // Act
            var result = Parcel.MigrateParcel(
                new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()),
                command.OldParcelId,
                command.NewParcelId,
                command.CaPaKey,
                command.ParcelStatus,
                command.IsRemoved,
                command.AddressPersistentLocalIds,
                command.ExtendedWkbGeometry);

            // Assert
            result.Should().NotBeNull();
            result.ParcelId.Should().Be(command.NewParcelId);
            result.CaPaKey.Should().Be(command.CaPaKey);
            result.ParcelStatus.Should().Be(command.ParcelStatus);
            result.IsRemoved.Should().Be(command.IsRemoved);
            result.AddressPersistentLocalIds.Should().BeEquivalentTo(command.AddressPersistentLocalIds);
            result.Geometry.Should().Be(command.ExtendedWkbGeometry);
        }
    }
}
