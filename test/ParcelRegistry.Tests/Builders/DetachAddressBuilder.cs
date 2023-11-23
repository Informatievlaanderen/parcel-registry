namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class DetachAddressBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public DetachAddressBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public DetachAddressBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public DetachAddressBuilder WithAddress(int addressPersistentLocalId)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);

            return this;
        }

        public DetachAddress Build()
        {
            return new DetachAddress(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _fixture.Create<Provenance>());
        }
    }
}
