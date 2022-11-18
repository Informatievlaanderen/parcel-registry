namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class AddressPersistentLocalId : IntegerValueObject<AddressPersistentLocalId>
    {
        public AddressPersistentLocalId(int persistentLocalId) : base(persistentLocalId) { }
    }
}
