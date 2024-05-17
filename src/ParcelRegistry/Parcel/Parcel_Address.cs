namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Commands;
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

        public void ReaddressAddresses(
            IReadOnlyList<ReaddressData> readdresses)
        {
            var addressPersistentLocalIdsToAttach = readdresses
                .Select(x => x.DestinationAddressPersistentLocalId)
                .Except(readdresses.Select(x => x.SourceAddressPersistentLocalId))
                .Except(AddressPersistentLocalIds)
                .ToList();

            var addressPersistentLocalIdsToDetach = readdresses
                .Select(x => x.SourceAddressPersistentLocalId)
                .Except(readdresses.Select(x => x.DestinationAddressPersistentLocalId))
                .Where(AddressPersistentLocalIds.Contains)
                .ToList();

            if (!addressPersistentLocalIdsToAttach.Any() && !addressPersistentLocalIdsToDetach.Any())
            {
                return;
            }

            ApplyChange(new ParcelAddressesWereReaddressed(
                ParcelId,
                CaPaKey,
                addressPersistentLocalIdsToAttach,
                addressPersistentLocalIdsToDetach,
                readdresses.Select(x => new AddressRegistryReaddress(x))));
        }
    }
}
