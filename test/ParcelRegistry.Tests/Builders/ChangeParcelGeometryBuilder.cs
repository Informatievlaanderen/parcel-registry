namespace ParcelRegistry.Tests.Builders
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    /// <summary>
    /// Builder for creating instances of ChangeParcelGeometryBuilder.
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class ChangeParcelGeometryBuilder
    {
        private readonly Fixture _fixture;
        private VbrCaPaKey? _vbrCaPaKey;
        private ExtendedWkbGeometry? _extendedWkbGeometry;
        private List<AddressPersistentLocalId> _addressPersistentLocalIds;

        public ChangeParcelGeometryBuilder(Fixture fixture)
        {
            _fixture = fixture;
            _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        }

        public ChangeParcelGeometryBuilder WithVbrCaPaKey(VbrCaPaKey caPaKey)
        {
            _vbrCaPaKey = caPaKey;

            return this;
        }

        public ChangeParcelGeometryBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ChangeParcelGeometryBuilder WithAddress(int address)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(address));

            return this;
        }

        public ChangeParcelGeometry Build()
        {
            return new ChangeParcelGeometry(
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                _addressPersistentLocalIds,
                _fixture.Create<Provenance>());
        }
    }
}
