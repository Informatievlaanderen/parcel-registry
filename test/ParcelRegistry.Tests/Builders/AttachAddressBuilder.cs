namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class AttachAddressBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _addressPersistentLocalId;

        public AttachAddressBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public AttachAddressBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public AttachAddressBuilder WithAddress(int persistentLocalId)
        {
            _addressPersistentLocalId = new AddressPersistentLocalId(persistentLocalId);

            return this;
        }

        public AttachAddress Build()
        {
            var attachAddress = new AttachAddress(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>(),
                _fixture.Create<Provenance>());

            return attachAddress;
        }
    }
}
