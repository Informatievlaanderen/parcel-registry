namespace ParcelRegistry.Tests.Fixtures
{
    using AutoFixture;
    using AutoFixture.Kernel;
    using ParcelRegistry.Parcel;

    public class WithExtendedWkbGeometryPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry();

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
