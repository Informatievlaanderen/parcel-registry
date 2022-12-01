namespace ParcelRegistry.Parcel
{
    using DataStructures;

    public interface IAddresses
    {
        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId);
    }
}
