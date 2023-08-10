namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Collections.Generic;
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
    using Parcel;
    using Parcel.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenChangeParcelGeometryRequestSent : ParcelRegistryTest
    {
        public GivenChangeParcelGeometryRequestSent(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

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

            var previouslyAttachedAddress = Fixture.Create<AddressPersistentLocalId>();
            fakeAddressConsumerContext.AddressConsumerItems.Add(
                new AddressConsumerItem(
                    previouslyAttachedAddress,
                    AddressStatus.Current,
                    "geometryMethod",
                    "geometrySpec",
                    GeometryHelpers.ValidPoint1InPolgyon3
                ));
            fakeAddressConsumerContext.SaveChanges();

            DispatchArrangeCommand(
                new ImportParcel(
                    new VbrCaPaKey(caPaKey.VbrCaPaKey),
                    GeometryHelpers.ValidGmlPolygon3.GmlToExtendedWkbGeometry(),
                    new List<AddressPersistentLocalId> {previouslyAttachedAddress},
                    Fixture.Create<Provenance>()));

            var changeParcelGeometryRequest = new ChangeParcelGeometryRequest(
                new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon2, 9, DateTime.Now));

            var sut = new ChangeParcelGeometryHandler(
                Container,
                fakeAddressConsumerContext);

            await sut.Handle(changeParcelGeometryRequest, CancellationToken.None);

            var parcels = Container.Resolve<IParcels>();
            var parcel = await parcels.GetOptionalAsync(new ParcelStreamId(parcelId));

            parcel.Should().NotBeNull();
            parcel.Value.Geometry.Should().Be(ExtendedWkbGeometry.CreateEWkb(changeParcelGeometryRequest.GrbParcel.Geometry.ToBinary()));

            parcel.Value.AddressPersistentLocalIds.Count.Should().Be(2);
            parcel.Value.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId1);
            parcel.Value.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId2);
            parcel.Value.AddressPersistentLocalIds.Should().NotContain(previouslyAttachedAddress);

            parcel.Value.LastProvenanceData.ToProvenance().Should().BeEquivalentTo(new Provenance(
                    parcel.Value.LastProvenanceData.Timestamp,
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelenplan"),
                    new Operator("Parcel Registry"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen));
        }
    }
}
