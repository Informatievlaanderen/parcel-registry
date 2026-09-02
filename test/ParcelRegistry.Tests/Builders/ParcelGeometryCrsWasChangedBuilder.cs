namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    /// <summary>
    /// Builder for creating instances of ParcelGeometryCrsWasChanged.
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class ParcelGeometryCrsWasChangedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private ExtendedWkbGeometry? _extendedWkbGeometry;

        public ParcelGeometryCrsWasChangedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelGeometryCrsWasChangedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelGeometryCrsWasChangedBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelGeometryCrsWasChangedBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ParcelGeometryCrsWasChanged Build()
        {
            var parcelGeometryCrsWasChanged = new ParcelGeometryCrsWasChanged(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelGeometryCrsWasChanged.SetFixtureProvenance(_fixture);

            return parcelGeometryCrsWasChanged;
        }
    }
}
