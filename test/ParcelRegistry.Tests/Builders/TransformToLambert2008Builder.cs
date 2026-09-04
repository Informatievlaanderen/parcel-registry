namespace ParcelRegistry.Tests.Builders
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Commands;

    /// <summary>
    /// Builder for creating instances of TransformToLambert2008.
    /// </summary>
    public class TransformToLambert2008Builder
    {
        private readonly Fixture _fixture;
        private ParcelId? _parcelId;

        public TransformToLambert2008Builder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public TransformToLambert2008Builder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;

            return this;
        }

        public TransformToLambert2008 Build()
        {
            return new TransformToLambert2008(
                _parcelId ?? _fixture.Create<ParcelId>(),
                _fixture.Create<Provenance>());
        }
    }
}
