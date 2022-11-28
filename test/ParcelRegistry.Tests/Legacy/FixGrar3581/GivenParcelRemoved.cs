namespace ParcelRegistry.Tests.Legacy.FixGrar3581
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Fixtures;
    using global::AutoFixture;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Commands.Fixes;
    using ParcelRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;
    using WithFixedParcelId = AutoFixture.WithFixedParcelId;

    public class GivenParcelRemoved : ParcelRegistryTest
    {
        public GivenParcelRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithNoDeleteModification());
        }

        [Fact]
        public void ThenNothingHappens()
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
                        .WithAddressId(addressIdToRemove),
                    Fixture.Create<ParcelWasRemoved>())
                .When(new FixGrar3581(parcelId, ParcelStatus.Retired, new List<AddressId> { commonAddressId, addressIdToAdd }))
                .ThenNone());
        }
    }
}
