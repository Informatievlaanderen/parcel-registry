namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Extensions;
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
    using NetTopologySuite.Geometries;
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

            var importRequest = new ImportOrUpdateParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon2, 9, DateTime.Now));

            var sut = new ImportOrUpdateParcelHandler(Container, fakeAddressConsumerContext);

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

        /// <summary>
        /// In ConsumerAddressContext.FindAddressesWithinGeometry uses the NTS library to find addresses within a geometry and uses the FixGeometry method.
        /// </summary>
        [Fact]
        public void VerifyThatFixedGeometryIsApproximatelyTheOriginal()
        {
            var polygon = GmlHelpers.CreateGmlReader().Read(GeometryHelpers.InValidNTSButValidSqlPolygon);
            polygon.Should().BeOfType<Polygon>();
            polygon.IsValid.Should().BeFalse();
            GeometryValidator.IsValid(polygon).Should().BeTrue();

            var fixedPolygon = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(polygon);
            fixedPolygon.Area.Should().BeApproximately(polygon.Area, 0.00000000001); //In this case: 12166.896162859213, but found 12166.896162859215
        }
    }
}
