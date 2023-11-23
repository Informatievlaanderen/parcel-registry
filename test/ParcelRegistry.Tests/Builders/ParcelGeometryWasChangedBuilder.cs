namespace ParcelRegistry.Tests.Builders
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    /// <summary>
    /// Builder for creating instances of ParcelGeometryWasChangedBuilder.
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class ParcelGeometryWasChangedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private ExtendedWkbGeometry? _extendedWkbGeometry;

        public ParcelGeometryWasChangedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelGeometryWasChangedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelGeometryWasChangedBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelGeometryWasChangedBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ParcelGeometryWasChanged Build()
        {
            var parcelGeometryWasChanged = new ParcelGeometryWasChanged(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelGeometryWasChanged.SetFixtureProvenance(_fixture);

            return parcelGeometryWasChanged;
        }
    }
}
