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

    public class DetachAddressRequestValidatorTests
    {
        private readonly FakeConsumerAddressContext _addressContext;
        private readonly WKBReader _wkbReader;
        private readonly Fixture _fixture;

        private readonly DetachAddressRequestValidator _sut;

        public DetachAddressRequestValidatorTests()
        {
            _addressContext = new FakeConsumerAddressContextFactory().CreateDbContext(Array.Empty<string>());
            _wkbReader = new WKBReader(
                new NtsGeometryServices(
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                    new PrecisionModel(PrecisionModels.Floating),
                    WkbGeometry.SridLambert72));
            _fixture = new Fixture();
            _fixture.Customize(new WithExtendedWkbGeometry());

            _sut = new DetachAddressRequestValidator(_addressContext);
        }

        [Fact]
        public void WhenAdresIdPuriInvalid_ThenValidationException()
        {
            var result = _sut.TestValidate(
                new DetachAddressRequest()
                {
                    AdresId = "Invalid Puri"
                });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(DetachAddressRequest.AdresId))
                .WithErrorCode("PerceelAdresOngeldig")
                .WithErrorMessage("Ongeldig adresId.");
        }

        [Fact]
        public void WhenAddresConsumerItemIsNotFound_ThenValidationException()
        {
            var result = _sut.TestValidate(
                new DetachAddressRequest()
                {
                    AdresId = PuriCreator.CreateAdresId(123)
                });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(DetachAddressRequest.AdresId))
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

            var result = _sut.TestValidate(new DetachAddressRequest{AdresId = addressPersistentLocalId });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(DetachAddressRequest.AdresId))
                .WithErrorCode("PerceelAdresOngeldig")
                .WithErrorMessage("Ongeldig adresId.");
        }
    }
}
