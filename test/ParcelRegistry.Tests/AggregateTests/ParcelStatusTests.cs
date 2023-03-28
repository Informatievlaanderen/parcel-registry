namespace ParcelRegistry.Tests.AggregateTests
{
    using System;
    using FluentAssertions;
    using Parcel;
    using Xunit;

    public class ParcelStatusTests
    {
        [Fact]
        public void GivenNonParseableString_ThrowsNotImplementedException()
        {
            Action act = () => ParcelStatus.Parse("bla");
            act.Should().Throw<NotImplementedException>();
        }
    }
}
