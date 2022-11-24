namespace ParcelRegistry.Parcel
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Newtonsoft.Json;

    public class ParcelId : GuidValueObject<ParcelId>
    {
        private static readonly Guid NamespaceId = new Guid("c585548d-2ff1-4f8e-ba05-da631567c0b8");

        public static ParcelId CreateFor(VbrCaPaKey vbrCaPaKey) => new ParcelId(
            Deterministic.Create(
                NamespaceId,
                vbrCaPaKey.CreateDeterministicId().ToString("D")));

        public ParcelId([JsonProperty("value")] Guid parcelId) : base(parcelId) { }
    }
}
