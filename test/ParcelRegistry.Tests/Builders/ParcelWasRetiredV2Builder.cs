namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using EventExtensions;
    using Parcel;
    using Parcel.Events;

    public class ParcelWasRetiredV2Builder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;
        private VbrCaPaKey? _vbrCaPaKey;

        public ParcelWasRetiredV2Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public ParcelWasRetiredV2Builder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public ParcelWasRetiredV2Builder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public ParcelWasRetiredV2 Build()
        {
            var parcelWasRetiredV2 = new ParcelWasRetiredV2(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>());
            parcelWasRetiredV2.SetFixtureProvenance(_fixture);

            return parcelWasRetiredV2;
        }
    }
}
