namespace ParcelRegistry.Parcel.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("CRAB-TerrainObjectWasImported")]
    public class TerrainObjectWasImportedFromCrab
    {
        public int TerrainObjectId { get; }
        public string IdentifierTerrainObject { get; }
        public string TerrainObjectNatureCode { get; }
        public decimal? XCoordinate { get; }
        public decimal? YCoordinate { get; }
        public string BuildingNature { get; }
        public LocalDateTime? Begin { get; }
        public LocalDateTime? End { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public TerrainObjectWasImportedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabIdentifierTerrainObject identifierTerrainObject,
            CrabTerrainObjectNatureCode terrainObjectNatureCode,
            CrabCoordinate xCoordinate,
            CrabCoordinate yCoordinate,
            CrabBuildingNature buildingNature,
            CrabLifetime crabLifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            IdentifierTerrainObject = identifierTerrainObject;
            TerrainObjectNatureCode = terrainObjectNatureCode;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            BuildingNature = buildingNature;
            Begin = crabLifetime.BeginDateTime;
            End = crabLifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private TerrainObjectWasImportedFromCrab(
            int terrainObjectId,
            string identifierTerrainObject,
            string terrainObjectNatureCode,
            decimal? xCoordinate,
            decimal? yCoordinate,
            string buildingNature,
            LocalDateTime? begin,
            LocalDateTime? end,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabIdentifierTerrainObject(identifierTerrainObject),
                new CrabTerrainObjectNatureCode(terrainObjectNatureCode),
                xCoordinate.HasValue ? new CrabCoordinate(xCoordinate.Value) : null,
                yCoordinate.HasValue ? new CrabCoordinate(yCoordinate.Value) : null,
                new CrabBuildingNature(buildingNature),
                new CrabLifetime(begin, end),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
