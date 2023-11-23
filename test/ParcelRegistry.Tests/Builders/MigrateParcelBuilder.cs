namespace ParcelRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;
    using ParcelId = ParcelRegistry.Legacy.ParcelId;
    using ParcelStatus = ParcelRegistry.Legacy.ParcelStatus;

    /// <summary>
    /// Builder for creating instances of MigrateParcelBuilder.
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class MigrateParcelBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _caPaKey;
        private ParcelStatus? _status;
        private bool _isRemoved;
        private readonly List<AddressPersistentLocalId> _addressPersistentLocalIds;
        private ExtendedWkbGeometry? _extendedWkbGeometry;

        public MigrateParcelBuilder(Fixture fixture)
        {
            _fixture = fixture;
            _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        }

        public MigrateParcelBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public MigrateParcelBuilder WithCaPaKey(VbrCaPaKey caPaKey)
        {
            _caPaKey = caPaKey;

            return this;
        }

        public MigrateParcelBuilder WithStatus(ParcelStatus status)
        {
            _status = status;

            return this;
        }

        public MigrateParcelBuilder WithIsRemoved()
        {
            _isRemoved = true;

            return this;
        }

        public MigrateParcelBuilder WithAddress(int persistentLocalId)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(persistentLocalId));

            return this;
        }

        public MigrateParcelBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public MigrateParcel Build()
        {
            return new MigrateParcel(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _caPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _status ?? _fixture.Create<ParcelStatus>(),
                _isRemoved,
                _addressPersistentLocalIds,
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                _fixture.Create<Provenance>());
        }
    }
}
