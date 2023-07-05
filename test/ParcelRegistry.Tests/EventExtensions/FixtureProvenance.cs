namespace ParcelRegistry.Tests.EventExtensions
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;

    public static class FixtureProvenance
    {
        public static void SetFixtureProvenance(this IParcelEvent @event, Fixture fixture)
        {
            @event.SetProvenance(fixture.Create<Provenance>());
        }
    }
}
