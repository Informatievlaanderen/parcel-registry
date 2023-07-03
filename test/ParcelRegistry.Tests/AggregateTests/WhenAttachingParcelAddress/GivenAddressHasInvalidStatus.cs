namespace ParcelRegistry.Tests.AggregateTests.WhenAttachingParcelAddress
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Consumer.Address;
    using Fixtures;
    using NetTopologySuite.Geometries;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using Coordinate = Parcel.Coordinate;

    public class GivenAddressHasInvalidStatus : ParcelRegistryTest
    {
        public GivenAddressHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
            Fixture.Customize(new WithExtendedWkbGeometryPoint());
        }

        [Theory]
        [InlineData("Rejected")]
        [InlineData("Retired")]
        public void ThenThrowAddressHasInvalidStatusException(string addressStatus)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(11111111);

            var command = new AttachAddress(
                Fixture.Create<ParcelId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    new AddressPersistentLocalId(123),
                    new AddressPersistentLocalId(456),
                    new AddressPersistentLocalId(789),
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Parse(addressStatus),
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }
    }
}
