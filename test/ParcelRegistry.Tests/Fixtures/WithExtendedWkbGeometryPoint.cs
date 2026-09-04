namespace ParcelRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using ParcelRegistry.Parcel;

    /// <summary>
    /// Generates address positions in Lambert 72 (EPSG 31370). The coordinates lie inside Flanders because
    /// the consumer refuses a position it cannot transform, and both Lambert transforms decide what to do by
    /// envelope rather than by SRID. The Lambert 2008 counterpart is
    /// <see cref="WithExtendedWkbGeometryPointLambert2008"/>. See ADR 0004.
    /// </summary>
    public class WithExtendedWkbGeometryPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var x = GeometryHelpers.Lambert72PointX + fixture.Create<uint>() % 1000;
            var y = GeometryHelpers.Lambert72PointY + fixture.Create<uint>() % 1000;

            var extendedWkbGeometry = GeometryHelpers.CreateFromWkt($"POINT ({x} {y})");

            fixture.Customize<ExtendedWkbGeometry>(c => c.FromFactory(
                () => new ExtendedWkbGeometry(extendedWkbGeometry.ToString())));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(extendedWkbGeometry.ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "extendedWkbGeometry")));
        }
    }
}
