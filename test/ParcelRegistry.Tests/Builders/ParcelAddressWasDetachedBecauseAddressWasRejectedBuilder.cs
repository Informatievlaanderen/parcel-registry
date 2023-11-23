namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder WithAddress(int address)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasDetachedBecauseAddressWasRejected Build()
        {
            var parcelAddressWasDetachedBecauseAddressWasRejected = new ParcelAddressWasDetachedBecauseAddressWasRejected(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>());
            parcelAddressWasDetachedBecauseAddressWasRejected.SetFixtureProvenance(_fixture);

            return parcelAddressWasDetachedBecauseAddressWasRejected;
        }
    }
}
