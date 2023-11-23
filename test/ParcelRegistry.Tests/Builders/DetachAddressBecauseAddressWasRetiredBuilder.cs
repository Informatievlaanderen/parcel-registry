namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class DetachAddressBecauseAddressWasRetiredBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public DetachAddressBecauseAddressWasRetiredBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public DetachAddressBecauseAddressWasRetiredBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public DetachAddressBecauseAddressWasRetiredBuilder WithAddress(int addressPersistentLocalId)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);

            return this;
        }

        public DetachAddressBecauseAddressWasRetired Build()
        {
            return new DetachAddressBecauseAddressWasRetired(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _fixture.Create<Provenance>());
        }
    }
}
