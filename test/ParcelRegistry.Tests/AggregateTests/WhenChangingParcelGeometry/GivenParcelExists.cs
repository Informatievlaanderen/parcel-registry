namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public void ThenParcelGeometryChanged()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var command = new ChangeParcelGeometry(
                caPaKey,
                GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry(),
                new List<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported)
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelGeometryWasChanged(parcelId, caPaKey,  GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry())));
        }

        [Fact]
        public void WithAddresses_ThenParcelAddressesAreAttachedAndDetached()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var addressPersistentLocalIdToAttach = new AddressPersistentLocalId(1);
            var addressPersistentLocalIdToDoNothing = new AddressPersistentLocalId(2);
            var addressPersistentLocalIdToDetach = new AddressPersistentLocalId(3);

            var toDoNothingParcelAddressWasAttached = new ParcelAddressWasAttachedV2(
                parcelId,
                caPaKey,
                addressPersistentLocalIdToDoNothing);
            toDoNothingParcelAddressWasAttached.SetFixtureProvenance(Fixture);

            var toDetachParcelAddressWasAttached = new ParcelAddressWasAttachedV2(
                parcelId,
                caPaKey,
                addressPersistentLocalIdToDetach);
            toDetachParcelAddressWasAttached.SetFixtureProvenance(Fixture);

            var command = new ChangeParcelGeometry(
                caPaKey,
                GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry(),
                new List<AddressPersistentLocalId>()
                {
                    addressPersistentLocalIdToAttach,
                    addressPersistentLocalIdToDoNothing
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId),
                    parcelWasImported,
                    toDoNothingParcelAddressWasAttached,
                    toDetachParcelAddressWasAttached)
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasDetachedV2(parcelId, caPaKey, addressPersistentLocalIdToDetach),
                    new ParcelAddressWasAttachedV2(parcelId, caPaKey, addressPersistentLocalIdToAttach),
                    new ParcelGeometryWasChanged(parcelId, caPaKey,  GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry())));
        }

        [Fact]
        public void WhenGeometryIsTheSame_ThenNone()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var extendedWkbGeometry = GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry();
            var command = new ChangeParcelGeometry(
                caPaKey,
                extendedWkbGeometry,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Retired,
                isRemoved: false,
                new List<AddressPersistentLocalId>(),
                extendedWkbGeometry);
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithInvalidPolygon_ThenThrowsPolygonIsInvalidException()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var command = new ChangeParcelGeometry(
                caPaKey,
                GeometryHelpers.GmlPointGeometry.GmlToExtendedWkbGeometry(),
                new List<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported)
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void StateCheck()
        {
            var parcelId = Fixture.Create<ParcelId>();
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var parcelGeometryWasChanged = new ParcelGeometryWasChanged(parcelId, caPaKey, GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry());
            parcelGeometryWasChanged.SetFixtureProvenance(Fixture);

            var parcel = new ParcelFactory(NoSnapshotStrategy.Instance,  new Mock<IAddresses>().Object).Create();
            parcel.Initialize(new object[]
            {
                parcelWasImported,
                parcelGeometryWasChanged
            });

            parcel.Geometry.Should().Be(GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry());
        }
    }
}
