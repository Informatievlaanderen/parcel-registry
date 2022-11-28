namespace ParcelRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Consumer.Address;
    using FluentValidation;

    public class AttachAddressRequestValidator : AbstractValidator<AttachAddressRequest>
    {
        public AttachAddressRequestValidator(ConsumerAddressContext addressContext)
        {
            RuleFor(x => x.AddressPersistentLocalId)
                .Must(addressPersistentLocalId =>
                {
                    var address = addressContext.AddressConsumerItems.Find(addressPersistentLocalId);
                    return address is not null && !address.IsRemoved;
                }).DependentRules(() =>
                {
                    RuleFor(x => x.AddressPersistentLocalId)
                        .Must(addressPersistentLocalId =>
                        {
                            var address = addressContext.AddressConsumerItems.Find(addressPersistentLocalId);
                            return address!.Status == AddressStatus.Current || address.Status == AddressStatus.Proposed;
                        })
                        .WithErrorCode(ValidationErrors.AttachAddress.InvalidAddressStatus.Code)
                        .WithMessage(ValidationErrors.AttachAddress.InvalidAddressStatus.Message);
                })
                .WithErrorCode(ValidationErrors.Common.AddressNotFound.Code)
                .WithMessage(ValidationErrors.Common.AddressNotFound.Message);
        }
    }
}
