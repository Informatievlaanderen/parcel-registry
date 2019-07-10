namespace ParcelRegistry.Projections.Syndication.Address
{
    public enum AddressEvent
    {
        AddressWasRegistered,
        AddressWasRemoved,
        AddressPersistentLocalIdentifierWasAssigned,

        AddressBecameIncomplete,
        AddressBecameComplete
    }
}
