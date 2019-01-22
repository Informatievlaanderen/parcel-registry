namespace ParcelRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabBoxNumberType : StringValueObject<CrabBoxNumberType>
    {
        public CrabBoxNumberType([JsonProperty("value")] string boxNumberType) : base(boxNumberType) { }
    }
}
