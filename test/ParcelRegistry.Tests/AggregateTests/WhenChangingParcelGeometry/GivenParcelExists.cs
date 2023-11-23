namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using FluentAssertions;
    using Moq;
    using Parcel;
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

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var command = new ChangeParcelGeometryBuilder(Fixture)
                .WithVbrCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry())
                .Build();

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

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var addressPersistentLocalIdToAttach = new AddressPersistentLocalId(1);
            var addressPersistentLocalIdToDoNothing = new AddressPersistentLocalId(2);
            var addressPersistentLocalIdToDetach = new AddressPersistentLocalId(3);

            var toDoNothingParcelAddressWasAttached = new ParcelAddressWasAttachedV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithAddress(addressPersistentLocalIdToDoNothing)
                .Build();

            var toDetachParcelAddressWasAttached = new ParcelAddressWasAttachedV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithAddress(addressPersistentLocalIdToDetach)
                .Build();

            var command = new ChangeParcelGeometryBuilder(Fixture)
                .WithVbrCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry())
                .WithAddress(addressPersistentLocalIdToAttach)
                .WithAddress(addressPersistentLocalIdToDoNothing)
                .Build();

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

            var command = new ChangeParcelGeometryBuilder(Fixture)
                .WithVbrCaPaKey(caPaKey)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Retired)
                .Build();

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

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var command = new ChangeParcelGeometryBuilder(Fixture)
                .WithVbrCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(GeometryHelpers.GmlPointGeometry.GmlToExtendedWkbGeometry())
                .Build();

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

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var parcelGeometryWasChanged = new ParcelGeometryWasChangedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithVbrCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry())
                .Build();

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
