namespace ParcelRegistry.Tests.WhenImportingSubaddressFromCrab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Parcel.Commands.Crab;
    using Parcel.Events.Crab;

    public static class ImportSubaddressFromCrabExtensions
    {
        public static AddressSubaddressWasImportedFromCrab ToLegacyEvent(this ImportSubaddressFromCrab command)
        {
            return new AddressSubaddressWasImportedFromCrab(
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithModification(this ImportSubaddressFromCrab command,
            CrabModification? modification)
        {
            return new ImportSubaddressFromCrab(
                command.CaPaKey,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithLifetime(this ImportSubaddressFromCrab command, CrabLifetime lifetime)
        {
            return new ImportSubaddressFromCrab(
                command.CaPaKey,
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithHouseNumberId(this ImportSubaddressFromCrab command, CrabHouseNumberId houseNumberId)
        {
            return new ImportSubaddressFromCrab(
                command.CaPaKey,
                command.SubaddressId,
                houseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithSubaddressId(this ImportSubaddressFromCrab command, CrabSubaddressId subaddressId)
        {
            return new ImportSubaddressFromCrab(
                command.CaPaKey,
                subaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
