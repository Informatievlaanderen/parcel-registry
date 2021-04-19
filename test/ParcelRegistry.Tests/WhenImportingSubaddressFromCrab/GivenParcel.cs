namespace ParcelRegistry.Tests.WhenImportingSubaddressFromCrab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcel : ParcelRegistryTest
    {
        private readonly ParcelId _parcelId;
        private readonly Fixture _fixture;

        public GivenParcel(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
        }

        [Fact]
        public void ThenLegacyEventIsAdded()
        {
            var command = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>())
                .When(command)
                .Then(_parcelId,
                    command.ToLegacyEvent()));
        }
    }
}
