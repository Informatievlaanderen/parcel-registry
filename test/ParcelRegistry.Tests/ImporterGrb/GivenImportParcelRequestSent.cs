namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Consumer.Address;
    using FluentAssertions;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenImportParcelRequestSent : ParcelRegistryTest
    {
        public GivenImportParcelRequestSent(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task ThenCommandDispatched_StateCheck()
        {
            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());
            var parcelId = ParcelId.CreateFor(new VbrCaPaKey(caPaKey.VbrCaPaKey));

            var fakeAddressConsumerContext = new FakeConsumerAddressContextFactory().CreateDbContext();

            var addressPersistentLocalId1 = Fixture.Create<AddressPersistentLocalId>();
            fakeAddressConsumerContext.AddressConsumerItems.Add(
                new AddressConsumerItem(
                    addressPersistentLocalId1,
                    AddressStatus.Current,
                    "geometryMethod",
                    "geometrySpec",
                    GeometryHelpers.ValidPoint1InPolgyon2
                ));

            var addressPersistentLocalId2 = Fixture.Create<AddressPersistentLocalId>();
            fakeAddressConsumerContext.AddressConsumerItems.Add(
                new AddressConsumerItem(
                    addressPersistentLocalId2,
                    AddressStatus.Current,
                    "geometryMethod",
                    "geometrySpec",
                    GeometryHelpers.ValidPointOnEdgeOfPolygon2
                ));
            fakeAddressConsumerContext.SaveChanges();

            var importRequest = new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon2, 9, new DateTime()));

            var sut = new ImportParcelHandler(Container, fakeAddressConsumerContext);

            await sut.Handle(importRequest, CancellationToken.None);

            var parcels = Container.Resolve<IParcels>();
            var parcel = await parcels.GetOptionalAsync(new ParcelStreamId(parcelId));

            parcel.Should().NotBeNull();
            parcel.Value.Geometry.Should().Be(ExtendedWkbGeometry.CreateEWkb(importRequest.GrbParcel.Geometry.ToBinary()));

            parcel.Value.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId1);
            parcel.Value.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId2);

            parcel.Value.LastProvenanceData.ToProvenance().Should().BeEquivalentTo(new Provenance(
                    parcel.Value.LastProvenanceData.Timestamp,
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelenplan"),
                    new Operator("Parcel Registry"),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen));
        }
    }
}
