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

    public class AttachAddressRequestValidatorTest
    {
        private readonly AttachAddressRequestValidator _sut;
        private readonly FakeConsumerAddressContext _addressContext;

        public AttachAddressRequestValidatorTest()
        {
            _addressContext = new FakeConsumerAddressContextFactory().CreateDbContext(Array.Empty<string>());
            _sut = new AttachAddressRequestValidator(_addressContext);
        }

        [Fact]
        public void WhenAddresConsumerItemIsNotFound_ThenValidationException()
        {
            var result = _sut.TestValidate(new AttachAddressRequest());

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AddressPersistentLocalId))
                .WithErrorCode("AdresOngeldig")
                .WithErrorMessage("Ongeldig AdresId.");
        }

        [Fact]
        public void WhenAddresConsumerItemIsRemoved_ThenValidationException()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);

            _addressContext.AddAddress(addressPersistentLocalId, AddressStatus.Current, isRemoved: true);

            var result = _sut.TestValidate(new AttachAddressRequest{AddressPersistentLocalId = addressPersistentLocalId });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AddressPersistentLocalId))
                .WithErrorCode("AdresOngeldig")
                .WithErrorMessage("Ongeldig AdresId.");
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("Rejected")]
        public void WhenAddresConsumerItemHasInvalidStatus_ThenValidationException(string addressStatus)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(1);

            _addressContext.AddAddress(addressPersistentLocalId, AddressStatus.Parse(addressStatus));

            var result = _sut.TestValidate(new AttachAddressRequest { AddressPersistentLocalId = addressPersistentLocalId });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AttachAddressRequest.AddressPersistentLocalId))
                .WithErrorCode("AdresAfgekeurdGehistoreerd")
                .WithErrorMessage("Enkel een voorgesteld of adres in gebruik kan gekoppeld worden.");
        }
    }
}
