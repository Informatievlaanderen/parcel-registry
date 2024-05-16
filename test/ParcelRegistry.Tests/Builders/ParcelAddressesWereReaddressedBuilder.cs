namespace ParcelRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;

    public class ParcelAddressesWereReaddressedBuilder(Fixture fixture)
    {
        private readonly List<AddressPersistentLocalId> _attachedAddressPersistentLocalIds = [];
        private readonly List<AddressPersistentLocalId> _detachedAddressPersistentLocalIds = [];
        private readonly List<AddressRegistryReaddress> _addressRegistryReaddresses = [];

        public ParcelAddressesWereReaddressedBuilder WithAttachedAddress(int addressPersistentLocalid)
        {
            _attachedAddressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalid));
            return this;
        }

        public ParcelAddressesWereReaddressedBuilder WithDetachedAddress(int addressPersistentLocalid)
        {
            _detachedAddressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalid));
            return this;
        }

        public ParcelAddressesWereReaddressedBuilder WithReaddress(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId)
        {
            return WithReaddress(new AddressRegistryReaddress(
                new ReaddressData(
                    new AddressPersistentLocalId(sourceAddressPersistentLocalId),
                    new AddressPersistentLocalId(destinationAddressPersistentLocalId))));
        }

        public ParcelAddressesWereReaddressedBuilder WithReaddress(AddressRegistryReaddress addressRegistryReaddress)
        {
            _addressRegistryReaddresses.Add(addressRegistryReaddress);
            return this;
        }

        public ParcelAddressesWereReaddressed Build()
        {
           var @event = new ParcelAddressesWereReaddressed(
                fixture.Create<ParcelId>(),
                fixture.Create<VbrCaPaKey>(),
                _attachedAddressPersistentLocalIds,
                _detachedAddressPersistentLocalIds,
                _addressRegistryReaddresses);

           @event.SetFixtureProvenance(fixture);

           return @event;
        }
    }
}
