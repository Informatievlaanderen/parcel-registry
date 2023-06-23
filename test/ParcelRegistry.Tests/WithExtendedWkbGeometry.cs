namespace ParcelRegistry.Tests
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using Consumer.Address;
    using Projections.Syndication.Address;

    public class WithExtendedWkbGeometry : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.CreateEwkbFrom(
                GeometryHelpers.CreateFromWkt($"POINT ({fixture.Create<uint>()} {fixture.Create<uint>()})"));

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
