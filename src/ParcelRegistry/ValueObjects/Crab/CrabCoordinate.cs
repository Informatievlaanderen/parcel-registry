namespace ParcelRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabCoordinate : DecimalValueObject<CrabCoordinate>
    {
        public CrabCoordinate([JsonProperty("value")] decimal coordinate) : base(coordinate) { }
    }
}
