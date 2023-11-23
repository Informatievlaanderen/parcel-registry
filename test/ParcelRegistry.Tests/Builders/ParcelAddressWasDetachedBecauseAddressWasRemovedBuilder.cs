namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasDetachedBecauseAddressWasRemovedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public ParcelAddressWasDetachedBecauseAddressWasRemovedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRemovedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRemovedBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRemovedBuilder WithAddress(int address)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRemoved Build()
        {
            var parcelAddressWasDetachedBecauseAddressWasRemoved = new ParcelAddressWasDetachedBecauseAddressWasRemoved(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>());
            parcelAddressWasDetachedBecauseAddressWasRemoved.SetFixtureProvenance(_fixture);

            return parcelAddressWasDetachedBecauseAddressWasRemoved;
        }
    }
}
