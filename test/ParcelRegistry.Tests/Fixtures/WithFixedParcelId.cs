namespace ParcelRegistry.Tests.Fixtures
{
    using System;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Parcel;

    public class WithFixedParcelId : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize(new WithValidVbrCaPaKey());

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
