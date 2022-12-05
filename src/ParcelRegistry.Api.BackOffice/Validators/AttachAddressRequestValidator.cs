namespace ParcelRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Consumer.Address;
    using FluentValidation;
    using Parcel;
    using AddressStatus = ParcelRegistry.Parcel.DataStructures.AddressStatus;

    public class AttachAddressRequestValidator : AbstractValidator<AttachAddressRequest>
    {
        public AttachAddressRequestValidator(ConsumerAddressContext addressContext)
        {
            RuleFor(x => x.AddressPersistentLocalId)
                .Must(addressPersistentLocalId =>
                {
                    var address = addressContext.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                    return address is not null && !address.Value.IsRemoved;
                }).DependentRules(() =>
                {
                    RuleFor(x => x.AddressPersistentLocalId)
                        .Must(addressPersistentLocalId =>
                        {
                            var address = addressContext.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                            return address.Value.Status == AddressStatus.Current || address.Value.Status == AddressStatus.Proposed;
                        })
                        .WithErrorCode(ValidationErrors.AttachAddress.InvalidAddressStatus.Code)
                        .WithMessage(ValidationErrors.AttachAddress.InvalidAddressStatus.Message);
                })
                .WithErrorCode(ValidationErrors.Common.AddressNotFound.Code)
                .WithMessage(ValidationErrors.Common.AddressNotFound.Message);
        }
    }
}
