namespace ParcelRegistry.Tests.Legacy.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using ParcelRegistry.Legacy.Commands.Crab;
    using ParcelRegistry.Legacy.Events.Crab;

    public static class ImportTerrainObjectHouseNumberFromCrabExtensions
    {
        public static TerrainObjectHouseNumberWasImportedFromCrab ToLegacyEvent(this ImportTerrainObjectHouseNumberFromCrab command)
        {
            return new TerrainObjectHouseNumberWasImportedFromCrab(
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithModification(this ImportTerrainObjectHouseNumberFromCrab command,
            CrabModification? modification)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.CaPaKey,
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithLifetime(this ImportTerrainObjectHouseNumberFromCrab command, CrabLifetime lifetime)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.CaPaKey,
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithTerrainObjectHouseNumberId(
            this ImportTerrainObjectHouseNumberFromCrab command,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.CaPaKey,
                terrainObjectHouseNumberId,
                command.TerrainObjectId,
                command.HouseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportTerrainObjectHouseNumberFromCrab WithHouseNumberId(
            this ImportTerrainObjectHouseNumberFromCrab command,
            CrabHouseNumberId houseNumberId)
        {
            return new ImportTerrainObjectHouseNumberFromCrab(
                command.CaPaKey,
                command.TerrainObjectHouseNumberId,
                command.TerrainObjectId,
                houseNumberId,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

    }
}
