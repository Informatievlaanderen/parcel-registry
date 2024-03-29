namespace ParcelRegistry.Api.BackOffice.Validators
{
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Consumer.Address;
    using FluentValidation;
    using Parcel;
    using Abstractions.Extensions;
    using AddressStatus = Parcel.DataStructures.AddressStatus;

    public class AttachAddressRequestValidator : AbstractValidator<AttachAddressRequest>
    {
        public AttachAddressRequestValidator(ConsumerAddressContext addressContext)
        {
            RuleFor(x => x.AdresId)
              .Must(adresId =>
                  OsloPuriValidator.TryParseIdentifier(adresId, out var id)
                  || int.TryParse(id, out var persistentLocalId))
              .DependentRules(() =>
              {
                  RuleFor(x => x.AdresId)
                      .Must(adresId =>
                      {
                          var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(adresId);

                          var address = addressContext.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                          return address is not null && !address.Value.IsRemoved;
                      }).DependentRules(() =>
                      {
                          RuleFor(x => x.AdresId)
                              .Must(adresId =>
                              {
                                  var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(adresId);

                                  var address = addressContext.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                                  return address.Value.Status == AddressStatus.Current || address.Value.Status == AddressStatus.Proposed;
                              })
                              .WithErrorCode(ValidationErrors.AttachAddress.InvalidAddressStatus.Code)
                              .WithMessage(ValidationErrors.AttachAddress.InvalidAddressStatus.Message);
                      })
                      .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code)
                      .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message);
              })
              .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message)
              .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code);
        }
    }
}
