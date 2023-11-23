namespace ParcelRegistry.Tests.AggregateTests.WhenMigratingParcel
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Parcel;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;
    using ParcelStatus = ParcelRegistry.Legacy.ParcelStatus;

    public class GivenParcelAlreadyExists : ParcelRegistryTest
    {
        public GivenParcelAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowInvalidOperationException()
        {
            var parcelWasMigrated = Fixture.Create<ParcelWasMigrated>();

            var command = new MigrateParcelBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.NewParcelId),
                    parcelWasMigrated)
                .When(command)
                .Throws(new AggregateSourceException($"Parcel with id {command.NewParcelId} already exists")));
        }
    }
}
