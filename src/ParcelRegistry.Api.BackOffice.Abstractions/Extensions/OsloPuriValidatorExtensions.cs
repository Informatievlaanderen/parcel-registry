namespace ParcelRegistry.Api.BackOffice.Abstractions.Extensions
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;

    public static class OsloPuriValidatorExtensions
    {
        /// <exception cref="InvalidOperationException"></exception>
        public static int ParsePersistentLocalId(string url)
        {
            if (OsloPuriValidator.TryParseIdentifier(url, out var stringId) && int.TryParse(stringId, out int persistentLocalId))
            {
                return persistentLocalId;
            }

            throw new InvalidOperationException();
        }
    }
}
