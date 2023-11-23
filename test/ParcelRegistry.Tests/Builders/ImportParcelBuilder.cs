namespace ParcelRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    /// <summary>
    /// Builder for creating instances of ImportParcelBuilderBuilder
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class ImportParcelBuilder
    {
        private readonly Fixture _fixture;
        private VbrCaPaKey? _caPaKey;
        private ExtendedWkbGeometry? _extendedWkbGeometry;
        private List<AddressPersistentLocalId> _addressPersistentLocalIds;

        public ImportParcelBuilder(Fixture fixture)
        {
            _fixture = fixture;
            _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        }

        public ImportParcelBuilder WithAddress(int addressPersistentLocalId)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));

            return this;
        }

        public ImportParcelBuilder WithCaPaKey(VbrCaPaKey caPaKey)
        {
            _caPaKey = caPaKey;

            return this;
        }

        public ImportParcelBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ImportParcel Build()
        {
            return new ImportParcel(
                _caPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                _addressPersistentLocalIds,
                _fixture.Create<Provenance>());
        }
    }
}
