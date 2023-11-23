namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasDetachedBecauseAddressWasRetiredBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public ParcelAddressWasDetachedBecauseAddressWasRetiredBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRetiredBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRetiredBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRetiredBuilder WithAddress(int address)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRetired Build()
        {
            var parcelAddressWasDetachedBecauseAddressWasRetired = new ParcelAddressWasDetachedBecauseAddressWasRetired(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>());
            parcelAddressWasDetachedBecauseAddressWasRetired.SetFixtureProvenance(_fixture);

            return parcelAddressWasDetachedBecauseAddressWasRetired;
        }
    }
}
