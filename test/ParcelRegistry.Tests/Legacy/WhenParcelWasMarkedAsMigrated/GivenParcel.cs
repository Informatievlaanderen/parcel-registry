namespace ParcelRegistry.Tests.Legacy.WhenParcelWasMarkedAsMigrated
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Fixtures;
    using global::AutoFixture;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands;
    using ParcelRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

    public class GivenParcel : ParcelRegistryTest
    {
        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public void ThenParcelWasMarkedAsMigrated()
        {
            var command = Fixture.Create<MarkParcelAsMigrated>();

            var parcelId = Fixture.Create<ParcelId>();

            Assert(new Scenario()
                .Given(parcelId, Fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(
                    new Fact(parcelId,
                        new ParcelWasMarkedAsMigrated(parcelId))));
        }

        [Fact]
        public void AndAlreadyMigrated_ThenNone()
        {
            var command = Fixture.Create<MarkParcelAsMigrated>();

            Assert(new Scenario()
                .Given(
                    Fixture.Create<ParcelId>(),
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasMarkedAsMigrated>())
                .When(command)
                .ThenNone());
        }
    }
}
