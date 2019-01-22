namespace ParcelRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class ParcelId : GuidValueObject<ParcelId>
    {
        public static ParcelId CreateFor(VbrCaPaKey vbrCaPaKey) => new ParcelId(vbrCaPaKey.CreateDeterministicId());

        public ParcelId(Guid parcelId) : base(parcelId) { }
    }
}
