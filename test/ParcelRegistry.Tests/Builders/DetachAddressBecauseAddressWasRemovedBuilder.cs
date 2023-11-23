namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class DetachAddressBecauseAddressWasRemovedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public DetachAddressBecauseAddressWasRemovedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public DetachAddressBecauseAddressWasRemovedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public DetachAddressBecauseAddressWasRemovedBuilder WithAddress(int addressPersistentLocalId)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);

            return this;
        }

        public DetachAddressBecauseAddressWasRemoved Build()
        {
            return new DetachAddressBecauseAddressWasRemoved(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _fixture.Create<Provenance>());
        }
    }
}
