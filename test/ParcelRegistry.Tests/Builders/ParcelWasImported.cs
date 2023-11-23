namespace ParcelRegistry.Tests.Builders
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    /// <summary>
    /// Builder for creating instances of ParcelWasImportedBuilder.
    /// By default, the ExtendedWkbGeometry is a valid GmlPolygon.
    /// </summary>
    public class ParcelWasImportedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _caPaKey;
        private ExtendedWkbGeometry? _extendedWkbGeometry;

        public ParcelWasImportedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelWasImportedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelWasImportedBuilder WithCaPaKey(VbrCaPaKey caPaKey)
        {
            _caPaKey = caPaKey;

            return this;
        }

        public ParcelWasImportedBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ParcelWasImported Build()
        {
            var parcelWasImported = new ParcelWasImported(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _caPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasImported.SetFixtureProvenance(_fixture);

            return parcelWasImported;
        }
    }
}
