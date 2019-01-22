namespace ParcelRegistry.Tests.AutoFixture
{
    using System;
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithFixedParcelId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var capakey =
                new SpecimenContext(fixture).Resolve(
                    new RegularExpressionRequest("^[0-9]{5}_[A-Z]_[0-9]{4}_[A-Z_0]_[0-9]{3}_[0-9]{2}$"));

            fixture.Customize<VbrCaPaKey>(c => c.FromFactory(() => new VbrCaPaKey(capakey.ToString())));

            var vbrCaPaKey = fixture.Create<VbrCaPaKey>();
            fixture.Customize<ParcelId>(c => c.FromFactory(() => ParcelId.CreateFor(vbrCaPaKey)));

            var parcelId = fixture.Create<ParcelId>();

            fixture.Register(() => parcelId);
            fixture.Register(() => vbrCaPaKey);

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(parcelId),
                    new ParameterSpecification(
                        typeof(Guid),
                        "parcelId")));

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(vbrCaPaKey),
                    new ParameterSpecification(
                        typeof(string),
                        "vbrCaPaKey")));
        }
    }
}
