namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class DetachAddressBecauseAddressWasRejectedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public DetachAddressBecauseAddressWasRejectedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public DetachAddressBecauseAddressWasRejectedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public DetachAddressBecauseAddressWasRejectedBuilder WithAddress(int addressPersistentLocalId)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);

            return this;
        }

        public DetachAddressBecauseAddressWasRejected Build()
        {
            return new DetachAddressBecauseAddressWasRejected(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _fixture.Create<Provenance>());
        }
    }
}
