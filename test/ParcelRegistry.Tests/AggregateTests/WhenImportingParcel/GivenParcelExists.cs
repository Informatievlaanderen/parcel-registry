namespace ParcelRegistry.Tests.AggregateTests.WhenImportingParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Builders;
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
    using ParcelStatus = Parcel.ParcelStatus;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void WithRealizedParcel_ThenThrowsParcelAlreadyExistsException()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            var command = new ImportParcel(
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                new List<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(command.ParcelId), parcelWasMigrated)
                .When(command)
                .Throws(new ParcelAlreadyExistsException(caPaKey)));
        }

        [Fact]
        public void WithRetiredParcel_ThenParcelRetirementWasCorrected()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Retired)
                .Build();

            var command = new ImportParcel(
                caPaKey,
                GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry(),
                new List<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelWasCorrectedFromRetiredToRealized(
                        command.ParcelId,
                        caPaKey,
                        command.ExtendedWkbGeometry)
                ));
        }

        [Fact]
        public void StateCheck()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .Build();

            var parcelWasRetiredV2 = new ParcelWasRetiredV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithVbrCaPaKey(caPaKey)
                .Build();

            var parcelRetirementWasCorrected = new ParcelWasCorrectedFromRetiredToRealizedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithVbrCaPaKey(caPaKey)
                .WithExtendedWkbGeometry(GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry())
                .Build();

            var parcel = new ParcelFactory(NoSnapshotStrategy.Instance,  new Mock<IAddresses>().Object).Create();
            parcel.Initialize(new object[]
            {
                parcelWasImported,
                parcelWasRetiredV2,
                parcelRetirementWasCorrected
            });

            // Assert
            parcel.Should().NotBeNull();
            parcel.ParcelId.Should().Be(parcelId);
            parcel.CaPaKey.Should().Be(caPaKey);
            parcel.ParcelStatus.Should().Be(ParcelStatus.Realized);
            parcel.IsRemoved.Should().BeFalse();
            parcel.AddressPersistentLocalIds.Should().BeEquivalentTo(new List<AddressPersistentLocalId>());
            parcel.Geometry.Should().Be(GeometryHelpers.ValidGmlPolygon2.GmlToExtendedWkbGeometry());
        }
    }
}
