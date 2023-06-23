namespace ParcelRegistry.Tests.BackOffice.Validators
{
    using System;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Consumer.Address;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Validators;
    using Xunit;

    public class AttachAddressRequestValidatorTests
    {
        private readonly FakeConsumerAddressContext _addressContext;
        private readonly WKBReader _wkbReader;
        private readonly Fixture _fixture;

        private readonly AttachAddressRequestValidator _sut;

        public AttachAddressRequestValidatorTests()
        {
            _addressContext = new FakeConsumerAddressContextFactory().CreateDbContext(Array.Empty<string>());
            _wkbReader = new WKBReader(
                new NtsGeometryServices(
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                    new PrecisionModel(PrecisionModels.Floating),
                    WkbGeometry.SridLambert72));
            _fixture = new Fixture();
            _fixture.Customize(new WithExtendedWkbGeometry());

            _sut = new AttachAddressRequestValidator(_addressContext);
        }

        [Fact]
        public void WhenAdresIdPuriInvalid_ThenValidationException()
        {
            var result = _sut.TestValidate(
                new AttachAddressRequest()
                {
                    AdresId = "Invalid Puri"
                });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AdresId))
                .WithErrorCode("PerceelAdresOngeldig")
                .WithErrorMessage("Ongeldig adresId.");
        }

        [Fact]
        public void WhenAddresConsumerItemIsNotFound_ThenValidationException()
        {
            var result = _sut.TestValidate(
                new AttachAddressRequest()
                {
                    AdresId = PuriCreator.CreateAdresId(123)
                });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AdresId))
                .WithErrorCode("PerceelAdresOngeldig")
                .WithErrorMessage("Ongeldig adresId.");
        }

        [Fact]
        public void WhenAddresConsumerItemIsRemoved_ThenValidationException()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);

            _addressContext.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Current,
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(_fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()),
                isRemoved: true);

            var result = _sut.TestValidate(new AttachAddressRequest{AdresId = addressPersistentLocalId });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AdresId))
                .WithErrorCode("PerceelAdresOngeldig")
                .WithErrorMessage("Ongeldig adresId.");
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("Rejected")]
        public void WhenAddresConsumerItemHasInvalidStatus_ThenValidationException(string addressStatus)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);
            var adresId = PuriCreator.CreateAdresId(addressPersistentLocalId);

            _addressContext.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Parse(addressStatus),
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(_fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            var result = _sut.TestValidate(new AttachAddressRequest { AdresId = adresId });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AdresId))
                .WithErrorCode("PerceelAdresAfgekeurdOfGehistoreerd")
                .WithErrorMessage("Het adres is afgekeurd of gehistoreerd.");
        }
    }
}
