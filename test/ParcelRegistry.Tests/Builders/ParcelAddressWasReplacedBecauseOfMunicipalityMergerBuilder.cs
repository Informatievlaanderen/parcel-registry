namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _newAddressPersistentLocalId;
        private AddressPersistentLocalId? _previousAddressPersistentLocalId;
        private VbrCaPaKey? _vbrCaPaKey;

        public ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithNewAddress(int address)
        {
            _newAddressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithPreviousAddress(int address)
        {
            _previousAddressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ParcelAddressWasReplacedBecauseOfMunicipalityMerger Build()
        {
           var ParcelAddressWasReplacedBecauseOfMunicipalityMerger = new ParcelAddressWasReplacedBecauseOfMunicipalityMerger(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _newAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _previousAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>());

           ParcelAddressWasReplacedBecauseOfMunicipalityMerger.SetFixtureProvenance(_fixture);

           return ParcelAddressWasReplacedBecauseOfMunicipalityMerger;
        }
    }
}
