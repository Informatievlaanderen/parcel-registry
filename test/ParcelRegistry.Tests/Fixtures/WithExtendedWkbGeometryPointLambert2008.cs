namespace ParcelRegistry.Tests
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using Parcel;

    /// <summary>
    /// Generates address positions in Lambert 2008 (EPSG 3812), as the address event store will hold them
    /// after its conversion. The Lambert 72 counterpart is <see cref="WithExtendedWkbGeometryPoint"/>.
    /// See ADR 0004.
    /// </summary>
    public class WithExtendedWkbGeometryPointLambert2008 : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var x = GeometryHelpers.Lambert2008PointX + fixture.Create<uint>() % 1000;
            var y = GeometryHelpers.Lambert2008PointY + fixture.Create<uint>() % 1000;

            var extendedWkbGeometry = GeometryHelpers.CreateEwkbPointLambert2008(x, y);

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
