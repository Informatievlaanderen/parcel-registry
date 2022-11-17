namespace ParcelRegistry.Legacy.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("CRAB-TerrainObjectHouseNumberWasImported")]
    [EventDescription("Legacy event om tblterreinobject_huisnummer en tblterreinobject_huisnummer_hist te importeren voor kadPercelen.")]
    public class TerrainObjectHouseNumberWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>
    {
        [EventPropertyDescription("Unieke sleutel.")]
        public int Key => TerrainObjectHouseNumberId;
        
        [EventPropertyDescription("CRAB-identificator van de terreinobject-huisnummerrelatie.")]
        public int TerrainObjectHouseNumberId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het huisnummer.")]
        public int HouseNumberId { get; }
        
        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDateTime { get; }
        
        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? EndDateTime { get; }
        
        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")]
        public Instant Timestamp { get; }
        
        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }
        
        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")] 
        public CrabModification? Modification { get; }
        
        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
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
