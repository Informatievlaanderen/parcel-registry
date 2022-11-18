namespace ParcelRegistry.Tests.AggregateTests.WhenMigratingParcel
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelDoesNotExist : ParcelRegistryTest
    {
        public GivenParcelDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenParcelWasMigratedEvent()
        {
            var command = new MigrateParcel(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelWasMigrated(
                        command.ParcelId,
                        command.ParcelStatus,
                        command.IsRemoved,
                        command.AddressPersistentLocalIds))));
        }

        [Fact]
        public void ThenParcelWasCorrectlyMutated()
        {
            var command = new MigrateParcel(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
                Fixture.Create<Provenance>());

            // Act
            var result = Parcel.MigrateParcel(
                new ParcelFactory(NoSnapshotStrategy.Instance),
                command.ParcelId,
                command.ParcelStatus,
                command.IsRemoved,
                command.AddressPersistentLocalIds);

            // Assert
            result.Should().NotBeNull();
            result.ParcelId.Should().Be(command.ParcelId);
            result.ParcelStatus.Should().Be(command.ParcelStatus);
            result.IsRemoved.Should().Be(command.IsRemoved);
            result.AddressPersistentLocalIds.Should().BeEquivalentTo(command.AddressPersistentLocalIds);
        }
    }
}
