namespace ParcelRegistry.Parcel
{
    using System.Linq;
    using DataStructures;
    using Events;
    using Exceptions;

    public sealed partial class Parcel
    {
        public void AttachAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardParcelNotRemoved();

            if (ParcelStatus != ParcelStatus.Realized)
            {
                throw new ParcelHasInvalidStatusException();
            }

            if (AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            var address = _addresses.GetOptional(addressPersistentLocalId);

            if (address is null)
            {
                throw new AddressNotFoundException();
            }

            if (address.Value.IsRemoved)
            {
                throw new AddressIsRemovedException();
            }

            var validStatuses = new[] { AddressStatus.Current, AddressStatus.Proposed };

            if (!validStatuses.Contains(address.Value.Status))
            {
                throw new AddressHasInvalidStatusException();
            }

            ApplyChange(new ParcelAddressWasAttachedV2(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddress(AddressPersistentLocalId addressPersistentLocalId)
        {
            GuardParcelNotRemoved();

            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedV2(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRemoved(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedBecauseAddressWasRemoved(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRejected(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedBecauseAddressWasRejected(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void DetachAddressBecauseAddressWasRetired(AddressPersistentLocalId addressPersistentLocalId)
        {
            if (!AddressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasDetachedBecauseAddressWasRetired(ParcelId, CaPaKey, addressPersistentLocalId));
        }

        public void ReplaceAttachedAddressBecauseAddressWasReaddressed(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId)
        {
            if (AddressPersistentLocalIds.Contains(addressPersistentLocalId)
                && !AddressPersistentLocalIds.Contains(previousAddressPersistentLocalId))
            {
                return;
            }

            ApplyChange(new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                ParcelId,
                CaPaKey,
                addressPersistentLocalId,
                previousAddressPersistentLocalId));
        }
    }
}
