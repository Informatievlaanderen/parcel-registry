using System;

namespace ParcelRegistry.Parcel.DataStructures
{ using System.Runtime.CompilerServices;
    public record struct AddressData(AddressPersistentLocalId AddressPersistentLocalId, AddressStatus Status, bool IsRemoved);

    public enum AddressStatus
    {
        Current,
        Proposed,
        Retired,
        Rejected
    }
}
