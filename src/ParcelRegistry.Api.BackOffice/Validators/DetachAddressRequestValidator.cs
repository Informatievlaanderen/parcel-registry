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

    public class DetachAddressRequestValidator : AbstractValidator<DetachAddressRequest>
    {
        public DetachAddressRequestValidator(ConsumerAddressContext addressContext)
        {
            RuleFor(x => x.AdresId)
              .Must(adresId =>
                  OsloPuriValidator.TryParseIdentifier(adresId, out var id)
                  || int.TryParse(id, out _))
              .DependentRules(() =>
              {
                  RuleFor(x => x.AdresId)
                      .Must(adresId =>
                      {
                          var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(adresId);

                          var address = addressContext.GetOptional(new AddressPersistentLocalId(addressPersistentLocalId));
                          return address is not null; //&& !address.Value.IsRemoved; GAWR-6746
                      })
                      .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code)
                      .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message);
              })
              .WithMessage(ValidationErrors.Common.AdresIdInvalid.Message)
              .WithErrorCode(ValidationErrors.Common.AdresIdInvalid.Code);
        }
    }
}
