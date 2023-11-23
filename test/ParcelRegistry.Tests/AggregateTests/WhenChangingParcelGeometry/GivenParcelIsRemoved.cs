namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Fixtures;
    using Parcel;
    using Parcel.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelIsRemoved : ParcelRegistryTest
    {
        public GivenParcelIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenThrowParcelIsRemovedException()
        {
            var command = new ChangeParcelGeometryBuilder(Fixture)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithParcelId(command.ParcelId)
                .WithStatus(ParcelStatus.Retired)
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new ParcelIsRemovedException(command.ParcelId)));
        }
    }
}
