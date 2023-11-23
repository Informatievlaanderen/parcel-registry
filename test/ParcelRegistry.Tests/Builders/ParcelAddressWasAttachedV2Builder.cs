namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasAttachedV2Builder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private AddressPersistentLocalId? _address;

        public ParcelAddressWasAttachedV2Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasAttachedV2Builder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasAttachedV2Builder WithCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasAttachedV2Builder WithAddress(int addressPersistentLocalId)
        {
            _address = new AddressPersistentLocalId(addressPersistentLocalId);

            return this;
        }

        public ParcelAddressWasAttachedV2 Build()
        {
            var parcelAddressWasAttachedV2 = new ParcelAddressWasAttachedV2(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _address ?? _fixture.Create<AddressPersistentLocalId>());

            parcelAddressWasAttachedV2.SetFixtureProvenance(_fixture);

            return parcelAddressWasAttachedV2;
        }
    }
}
