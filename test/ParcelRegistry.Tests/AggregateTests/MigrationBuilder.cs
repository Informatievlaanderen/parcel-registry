namespace ParcelRegistry.Tests.AggregateTests
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    public class MigrationBuilder
    {
        private readonly Fixture _fixture;

        private ParcelRegistry.Legacy.ParcelId? _legacyParcelId;
        private VbrCaPaKey? _caPaKey;
        private ParcelRegistry.Legacy.ParcelStatus? _status;
        private bool _isRemoved;

        public MigrationBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public MigrationBuilder WithCaPaKey(string capakey)
        {
            _caPaKey = new VbrCaPaKey(capakey);
            _legacyParcelId = ParcelRegistry.Legacy.ParcelId.CreateFor(_caPaKey);

            return this;
        }

        public MigrationBuilder WithStatus(ParcelRegistry.Legacy.ParcelStatus status)
        {
            _status = status;
            return this;
        }

        public MigrationBuilder WithRemoved()
        {
            _isRemoved = true;
            return this;
        }

        public MigrateParcel Build()
        {
            var capakey = _fixture.Create<VbrCaPaKey>();

            return new MigrateParcel(
                _legacyParcelId ?? ParcelRegistry.Legacy.ParcelId.CreateFor(capakey),
                _caPaKey ?? capakey,
                _status ?? _fixture.Create<ParcelRegistry.Legacy.ParcelStatus>(),
                isRemoved: _isRemoved,
                addressPersistentLocalIds: _fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry(),
                _fixture.Create<Provenance>());
        }
    }
}
