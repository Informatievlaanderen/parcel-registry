namespace ParcelRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class ParcelId : GuidValueObject<ParcelId>
    {
        public static ParcelId CreateFor(VbrCaPaKey vbrCaPaKey) => new ParcelId(vbrCaPaKey.CreateDeterministicId());

        public ParcelId([JsonProperty("value")] Guid parcelId) : base(parcelId) { }
    }
}
