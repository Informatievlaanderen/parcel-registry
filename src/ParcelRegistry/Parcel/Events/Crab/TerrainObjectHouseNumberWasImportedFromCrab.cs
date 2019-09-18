namespace ParcelRegistry.Parcel.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("CRAB-TerrainObjectHouseNumberWasImported")]
    public class TerrainObjectHouseNumberWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>
    {
        public int Key => TerrainObjectHouseNumberId;
        public int TerrainObjectHouseNumberId { get; }
        public int TerrainObjectId { get; }
        public int HouseNumberId { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
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
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
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
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
            : this(
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabTerrainObjectId(terrainObjectId),
                new CrabHouseNumberId(houseNumberId),
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
