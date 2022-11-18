namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class ParcelPersistentLocalId : IntegerValueObject<ParcelPersistentLocalId>
    {
        public ParcelPersistentLocalId(int persistentLocalId) : base(persistentLocalId) { }
    }
}
