namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _newAddressPersistentLocalId;
        private AddressPersistentLocalId? _previousAddressPersistentLocalId;
        private VbrCaPaKey? _vbrCaPaKey;

        public ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder WithNewAddress(int address)
        {
            _newAddressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder WithPreviousAddress(int address)
        {
            _previousAddressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasReplacedBecauseAddressWasReaddressed Build()
        {
           var parcelAddressWasReplacedBecauseAddressWasReaddressed = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _newAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _previousAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>());

           parcelAddressWasReplacedBecauseAddressWasReaddressed.SetFixtureProvenance(_fixture);

           return parcelAddressWasReplacedBecauseAddressWasReaddressed;
        }
    }
}
