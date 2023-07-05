namespace ParcelRegistry.Tests.AggregateTests.WhenImportingParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelDoesNotExists : ParcelRegistryTest
    {
        public GivenParcelDoesNotExists (ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenParcelImported()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var command = new ImportParcel(
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(command.ParcelId))
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelWasImported(
                        command.ParcelId,
                        caPaKey,
                        command.ExtendedWkbGeometry)));
        }

        [Fact]
        public void WithInvalidPolygon_ThenThrowsPolygonIsInvalidException()
        {
            var command = new ImportParcel(
                Fixture.Create<VbrCaPaKey>(),
                GeometryHelpers.GmlPointGeometry.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(command.ParcelId))
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void StateCheck()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var parcel = new ParcelFactory(NoSnapshotStrategy.Instance,  new Mock<IAddresses>().Object).Create();
            parcel.Initialize(new object[]
            {
                parcelWasImported
            });

            // Assert
            parcel.Should().NotBeNull();
            parcel.ParcelId.Should().Be(parcelId);
            parcel.CaPaKey.Should().Be(caPaKey);
            parcel.ParcelStatus.Should().Be(ParcelStatus.Realized);
            parcel.IsRemoved.Should().BeFalse();
            parcel.AddressPersistentLocalIds.Should().BeEquivalentTo(new List<AddressPersistentLocalId>());
            parcel.Geometry.Should().Be(GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
        }
    }
}
