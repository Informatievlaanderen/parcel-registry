namespace ParcelRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using ParcelRegistry.Parcel;

    /// <summary>
    /// Generates parcel geometries in Lambert 2008 (EPSG 3812), as the event store will hold them after the
    /// conversion. The Lambert 72 counterpart is <see cref="WithExtendedWkbGeometryPolygon"/>. See ADR 0003.
    /// </summary>
    public class WithExtendedWkbGeometryPolygonLambert2008 : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.ValidGmlPolygonLambert2008.ToExtendedWkbGeometryLambert2008();

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
