namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;

    public class RetireParcelV2Builder
    {
        private readonly Fixture _fixture;
        private VbrCaPaKey? _vbrCaPaKey;

        public RetireParcelV2Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public RetireParcelV2Builder WithVbrCaPaKey(VbrCaPaKey vbrCaPaKey)
        {
            _vbrCaPaKey = vbrCaPaKey;

            return this;
        }

        public RetireParcelV2 Build()
        {
           return new RetireParcelV2(
                _vbrCaPaKey ?? _fixture.Create<VbrCaPaKey>(),
                _fixture.Create<Provenance>());
        }
    }
}
