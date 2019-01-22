namespace ParcelRegistry.Parcel.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("CRAB-TerrainObjectHouseNumberWasImported")]
    public class TerrainObjectHouseNumberWasImportedFromCrab
    {
        public int TerrainObjectHouseNumberId { get; }
        public int TerrainObjectId { get; }
        public int HouseNumberId { get; }
        public LocalDateTime? Begin { get; }
        public LocalDateTime? End { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public TerrainObjectHouseNumberWasImportedFromCrab(
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabTerrainObjectId terrainObjectId,
            CrabHouseNumberId houseNumberId,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            TerrainObjectId = terrainObjectId;
            HouseNumberId = houseNumberId;
            Begin = lifetime.BeginDateTime;
            End = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private TerrainObjectHouseNumberWasImportedFromCrab(
            int terrainObjectHouseNumberId,
            int terrainObjectId,
            int houseNumberId,
            LocalDateTime? begin,
            LocalDateTime? end,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
            : this(
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabTerrainObjectId(terrainObjectId),
                new CrabHouseNumberId(houseNumberId),
                new CrabLifetime(begin, end),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
