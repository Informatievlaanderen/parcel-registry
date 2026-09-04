namespace ParcelRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using ParcelRegistry.Parcel;

    /// <summary>
    /// Generates parcel geometries as plain WKB carrying no SRID, the way they were persisted before the
    /// event store wrote EWKB. Readers must fall back to Lambert 72 for these. See ADR 0003.
    /// </summary>
    public class WithExtendedWkbGeometryPolygonWithoutSrid : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometryWithoutSrid();

            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(
                () => extendedWkbGeometry));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(extendedWkbGeometry.ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "extendedWkbGeometry")));
        }
    }
}
