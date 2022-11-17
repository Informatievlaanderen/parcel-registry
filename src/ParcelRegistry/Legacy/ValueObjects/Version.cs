namespace ParcelRegistry.Legacy
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class Version : IntegerValueObject<Version>
    {
        public Version([JsonProperty("value")] int version) : base(version) { }
    }
}
