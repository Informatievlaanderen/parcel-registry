namespace ParcelRegistry.Tests.AggregateTests.WhenRetiringParcel
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExistsWithAttachedAddresses : ParcelRegistryTest
    {
        public GivenParcelExistsWithAttachedAddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public void ThenParcelRetiredAndAddressesDetached()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImported(
                parcelId,
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(Fixture);

            var attachedAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var addressWasAttached = new ParcelAddressWasAttachedV2(parcelId, caPaKey, attachedAddressPersistentLocalId);
            addressWasAttached.SetFixtureProvenance(Fixture);

            var command = new RetireParcelV2(caPaKey, Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported, addressWasAttached)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                        new ParcelAddressWasDetachedV2(command.ParcelId, command.CaPaKey, attachedAddressPersistentLocalId)),
                    new Fact(new ParcelStreamId(command.ParcelId),
                        new ParcelWasRetiredV2(parcelId, caPaKey))
                ));
        }
    }
}
