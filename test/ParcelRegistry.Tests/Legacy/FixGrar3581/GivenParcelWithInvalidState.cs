namespace ParcelRegistry.Tests.Legacy.FixGrar3581
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands.Fixes;
    using ParcelRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithInvalidState : ParcelRegistryTest
    {
        public GivenParcelWithInvalidState(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
        }

        [Fact]
        public void ThenStateIsFixed()
        {
            var commonAddressId = Fixture.Create<AddressId>();
            var addressIdToRemove = Fixture.Create<AddressId>();
            var addressIdToAdd = Fixture.Create<AddressId>();

            var parcelId = Fixture.Create<ParcelId>();

            Assert(new Scenario()
                .Given(parcelId,
                    Fixture.Create<ParcelWasRegistered>(),
                    Fixture.Create<ParcelWasRealized>(),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(commonAddressId),
                    Fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressIdToRemove)) 
                .When(new FixGrar3581(parcelId, ParcelStatus.Retired, new List<AddressId> { commonAddressId, addressIdToAdd }))
                .Then(
                    new Fact(parcelId, new ParcelWasCorrectedToRetired(parcelId)),
                    new Fact(parcelId, new ParcelAddressWasDetached(parcelId, addressIdToRemove)),
                    new Fact(parcelId, new ParcelAddressWasAttached(parcelId, addressIdToAdd))));
        }
    }
}
