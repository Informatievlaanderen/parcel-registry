namespace ParcelRegistry.Tests.AggregateTests.WhenReplacingAddressBecauseOfMunicipalityMerger
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using FluentAssertions;
    using ParcelRegistry.Parcel;
    using ParcelRegistry.Tests.Builders;
    using ParcelRegistry.Tests.Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressNotAttached : ParcelRegistryTest
    {
        public GivenAddressNotAttached(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void StateCheck_OnlyPreviousWasAttached()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var newAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(previousAddressPersistentLocalId)
                .WithAddress(otherAddressPersistentLocalId)
                .Build();

            var @event = new ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder(Fixture)
                .WithVbrCaPaKey(new VbrCaPaKey(parcelWasMigrated.CaPaKey))
                .WithNewAddress(newAddressPersistentLocalId)
                .WithPreviousAddress(previousAddressPersistentLocalId)
                .Build();

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, @event });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().Contain(newAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().NotContain(previousAddressPersistentLocalId);
            sut.LastEventHash.Should().Be(@event.GetHash());
        }

        [Fact]
        public void StateCheck_BothPreviousAndNewWereAlreadyAttached()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var newAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(newAddressPersistentLocalId)
                .WithAddress(previousAddressPersistentLocalId)
                .WithAddress(otherAddressPersistentLocalId)
                .Build();

            var @event = new ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder(Fixture)
                .WithVbrCaPaKey(new VbrCaPaKey(parcelWasMigrated.CaPaKey))
                .WithNewAddress(newAddressPersistentLocalId)
                .WithPreviousAddress(previousAddressPersistentLocalId)
                .Build();

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, @event });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().Contain(newAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().NotContain(previousAddressPersistentLocalId);
            sut.LastEventHash.Should().Be(@event.GetHash());
        }
    }
}
