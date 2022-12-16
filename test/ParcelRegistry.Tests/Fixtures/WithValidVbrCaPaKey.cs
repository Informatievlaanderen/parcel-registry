namespace ParcelRegistry.Tests.Fixtures
{
    using System;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Parcel;

    public class WithValidVbrCaPaKey : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var capakey =
                new SpecimenContext(fixture).Resolve(
                    new RegularExpressionRequest("^[0-9]{5}_[A-Z]_[0-9]{4}_[A-Z_0]_[0-9]{3}_[0-9]{2}$"));

            fixture.Customize<VbrCaPaKey>(c => c.FromFactory(() => new VbrCaPaKey(capakey.ToString())));
        }
    }
}
