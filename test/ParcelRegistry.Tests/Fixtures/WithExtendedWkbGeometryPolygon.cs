namespace ParcelRegistry.Tests
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Parcel;

    public class WithExtendedWkbGeometryPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var extendedWkbGeometry = GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry();

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
