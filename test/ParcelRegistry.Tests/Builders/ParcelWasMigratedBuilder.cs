namespace ParcelRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    /// <summary>
    /// Builder for creating instances of ParcelWasMigrated.
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class ParcelWasMigratedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelRegistry.Legacy.ParcelId? _oldParcelId;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _caPaKey;
        private ParcelStatus? _status;
        private bool _isRemoved;
        private List<AddressPersistentLocalId> _addressPersistentLocalIds;
        private ExtendedWkbGeometry? _extendedWkbGeometry;

        public ParcelWasMigratedBuilder(Fixture fixture)
        {
            _fixture = fixture;
            _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        }

        public ParcelWasMigratedBuilder WithOldParcelId(ParcelRegistry.Legacy.ParcelId parcelId)
        {
            _oldParcelId = parcelId;

            return this;
        }

        public ParcelWasMigratedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelWasMigratedBuilder WithCaPaKey(VbrCaPaKey caPaKey)
        {
            _caPaKey = caPaKey;

            return this;
        }

        public ParcelWasMigratedBuilder WithStatus(ParcelStatus status)
        {
            _status = status;

            return this;
        }

        public ParcelWasMigratedBuilder WithIsRemoved()
        {
            _isRemoved = true;

            return this;
        }

        public ParcelWasMigratedBuilder WithAddress(int persistentLocalId)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(persistentLocalId));

            return this;
        }

        public ParcelWasMigratedBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ParcelWasMigrated Build()
        {
            var parcelWasMigrated = new ParcelWasMigrated(
                _oldParcelId ?? _fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                _parcelId ?? _fixture.Create<ParcelId>(),
                _caPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _status ?? _fixture.Create<ParcelStatus>(),
                _isRemoved,
                _addressPersistentLocalIds,
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());

            parcelWasMigrated.SetFixtureProvenance(_fixture);

            return parcelWasMigrated;
        }
    }
}
