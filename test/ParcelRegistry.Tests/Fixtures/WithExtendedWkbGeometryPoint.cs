namespace ParcelRegistry.Tests
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using Parcel;

    public class WithExtendedWkbGeometryPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers
                .CreateFromWkt($"POINT ({fixture.Create<uint>()} {fixture.Create<uint>()})");

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
