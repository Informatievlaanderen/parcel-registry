namespace ParcelRegistry.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Parcel;

    public class WithParcelStatus : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
            {
                var statuses = new List<ParcelStatus>
                {
                    ParcelStatus.Realized,
                    ParcelStatus.Retired
                };

                return statuses[new Random(fixture.Create<int>()).Next(0, statuses.Count - 1)];
            });
        }
    }
}
