namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _newAddressPersistentLocalId;
        private AddressPersistentLocalId? _previousAddressPersistentLocalId;

        public ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder WithNewAddress(int address)
        {
            _newAddressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ReplaceAttachedAddressBecauseAddressWasReaddressedBuilder WithPreviousAddress(int address)
        {
            _previousAddressPersistentLocalId = new AddressPersistentLocalId(address);

            return this;
        }

        public ReplaceAttachedAddressBecauseAddressWasReaddressed Build()
        {
            return new ReplaceAttachedAddressBecauseAddressWasReaddressed(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _newAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _previousAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _fixture.Create<Provenance>());
        }
    }
}
