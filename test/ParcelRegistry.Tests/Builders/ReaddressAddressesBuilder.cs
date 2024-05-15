namespace ParcelRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class ReaddressAddressesBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private readonly List<ReaddressData> _readdresses = [];

        public ReaddressAddressesBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ReaddressAddressesBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ReaddressAddressesBuilder WithReaddress(int sourceAddressPersistentLocalId, int destinationAddressPersistentLocalId)
        {
            _readdresses.Add(new ReaddressData(
                new AddressPersistentLocalId(sourceAddressPersistentLocalId),
                new AddressPersistentLocalId(destinationAddressPersistentLocalId)));

            return this;
        }

        public ReaddressAddresses Build()
        {
            return new ReaddressAddresses(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _readdresses,
                _fixture.Create<Provenance>());
        }
    }
}
