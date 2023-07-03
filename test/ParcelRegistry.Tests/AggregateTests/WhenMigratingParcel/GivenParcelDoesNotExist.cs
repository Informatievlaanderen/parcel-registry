namespace ParcelRegistry.Tests.AggregateTests.WhenMigratingParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelDoesNotExist : ParcelRegistryTest
    {
        public GivenParcelDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenParcelWasMigratedEvent()
        {
            var command = new MigrateParcel(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

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
                        command.XCoordinate,
                        command.YCoordinate,
                        command.ExtendedWkbGeometry))));
        }

        [Fact]
        public void StateCheck()
        {
            var command = new MigrateParcel(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            // Act
            var result = Parcel.MigrateParcel(
                new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()),
                command.OldParcelId,
                command.NewParcelId,
                command.CaPaKey,
                command.ParcelStatus,
                command.IsRemoved,
                command.AddressPersistentLocalIds,
                command.XCoordinate,
                command.YCoordinate,
                command.ExtendedWkbGeometry);

            // Assert
            result.Should().NotBeNull();
            result.ParcelId.Should().Be(command.NewParcelId);
            result.CaPaKey.Should().Be(command.CaPaKey);
            result.ParcelStatus.Should().Be(command.ParcelStatus);
            result.IsRemoved.Should().Be(command.IsRemoved);
            result.AddressPersistentLocalIds.Should().BeEquivalentTo(command.AddressPersistentLocalIds);
            result.XCoordinate.Should().Be(command.XCoordinate);
            result.YCoordinate.Should().Be(command.YCoordinate);
            result.Geometry.Should().Be(command.ExtendedWkbGeometry);
        }
    }
}
