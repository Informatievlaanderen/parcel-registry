namespace ParcelRegistry.Migrator.Lambert2008.Infrastructure.Modules
{
    using System;
    using Parcel;
    using Parcel.DataStructures;

    /// <summary>
    /// The <see cref="IAddresses"/> the parcel aggregate is constructed with. Only
    /// <see cref="Parcel.AttachAddress"/> and its detach counterparts read it, and the Lambert 2008 conversion
    /// dispatches neither, so being asked for an address here means the job is doing something it should not.
    /// </summary>
    internal sealed class UnreachableAddresses : IAddresses
    {
        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId)
            => throw new NotSupportedException(
                "The Lambert 2008 conversion does not read addresses.");
    }
}
