namespace ParcelRegistry.Tests.BackOffice.Validators
{
    using System;
    using Consumer.Address;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Validators;
    using Xunit;

    public class DetachAddressRequestValidatorTests
    {
        private readonly DetachAddressRequestValidator _sut;
        private readonly FakeConsumerAddressContext _addressContext;

        public DetachAddressRequestValidatorTests()
        {
            _addressContext = new FakeConsumerAddressContextFactory().CreateDbContext(Array.Empty<string>());
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
                .WithErrorCode("AdresOngeldig")
                .WithErrorMessage("Ongeldig AdresId.");
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
                .WithErrorCode("AdresOngeldig")
                .WithErrorMessage("Ongeldig AdresId.");
        }

        [Fact]
        public void WhenAddresConsumerItemIsRemoved_ThenValidationException()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);

            _addressContext.AddAddress(addressPersistentLocalId, AddressStatus.Current, isRemoved: true);

            var result = _sut.TestValidate(new DetachAddressRequest{AdresId = addressPersistentLocalId });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(DetachAddressRequest.AdresId))
                .WithErrorCode("AdresOngeldig")
                .WithErrorMessage("Ongeldig AdresId.");
        }
    }
}
