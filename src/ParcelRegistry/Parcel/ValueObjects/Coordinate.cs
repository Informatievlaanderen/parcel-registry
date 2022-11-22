namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class Coordinate : DecimalValueObject<Coordinate>
    {
        public Coordinate([JsonProperty("value")] decimal coordinate) : base(coordinate) { }
    }
}
