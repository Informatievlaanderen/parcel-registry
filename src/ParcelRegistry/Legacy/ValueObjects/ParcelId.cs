namespace ParcelRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class ParcelId : GuidValueObject<ParcelId>
    {
        public static ParcelId CreateFor(VbrCaPaKey vbrCaPaKey) => new ParcelId(vbrCaPaKey.CreateDeterministicId());

        public ParcelId([JsonProperty("value")] Guid parcelId) : base(parcelId) { }
    }
}
