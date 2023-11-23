namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasDetachedV2Builder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public ParcelAddressWasDetachedV2Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasDetachedV2Builder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasDetachedV2Builder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasDetachedV2Builder WithAddress(int address)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasDetachedV2 Build()
        {
            var parcelAddressWasDetachedV2 = new ParcelAddressWasDetachedV2(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>());
            parcelAddressWasDetachedV2.SetFixtureProvenance(_fixture);

            return parcelAddressWasDetachedV2;
        }
    }
}
