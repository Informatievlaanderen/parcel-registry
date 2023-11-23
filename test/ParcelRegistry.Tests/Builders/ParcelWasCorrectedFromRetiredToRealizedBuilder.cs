namespace ParcelRegistry.Tests.Builders
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelWasCorrectedFromRetiredToRealizedBuilder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;
        private ExtendedWkbGeometry? _extendedWkbGeometry;

        public ParcelWasCorrectedFromRetiredToRealizedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelWasCorrectedFromRetiredToRealizedBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelWasCorrectedFromRetiredToRealizedBuilder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelWasCorrectedFromRetiredToRealizedBuilder WithExtendedWkbGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            _extendedWkbGeometry = extendedWkbGeometry;

            return this;
        }

        public ParcelWasCorrectedFromRetiredToRealized Build()
        {
            var parcelWasCorrectedFromRetiredToRealized = new ParcelWasCorrectedFromRetiredToRealized(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _extendedWkbGeometry ?? GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            parcelWasCorrectedFromRetiredToRealized.SetFixtureProvenance(_fixture);

            return parcelWasCorrectedFromRetiredToRealized;
        }
    }
}
