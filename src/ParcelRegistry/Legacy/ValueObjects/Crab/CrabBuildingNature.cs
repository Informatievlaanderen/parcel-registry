namespace ParcelRegistry.Legacy
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabBuildingNature : StringValueObject<CrabBuildingNature>
    {
        public CrabBuildingNature([JsonProperty("value")] string buildingNature) : base(buildingNature) { }
    }
}
