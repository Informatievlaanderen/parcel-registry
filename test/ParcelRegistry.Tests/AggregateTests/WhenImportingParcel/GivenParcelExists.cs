namespace ParcelRegistry.Tests.AggregateTests.WhenImportingParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using Fixtures;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using ParcelId = ParcelRegistry.Legacy.ParcelId;
    using ParcelStatus = Parcel.ParcelStatus;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists (ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void WithAlreadyExistingParcel_ThenThrowsParcelAlreadyExistsException()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var legacyParcelId = ParcelId.CreateFor(caPaKey);
            var parcelId = ParcelRegistry.Parcel.ParcelId.CreateFor(caPaKey);

            var parcelWasMigrated = new ParcelWasMigrated(
                legacyParcelId,
                parcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                false,
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasMigrated.SetFixtureProvenance(Fixture);

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
    }
}
