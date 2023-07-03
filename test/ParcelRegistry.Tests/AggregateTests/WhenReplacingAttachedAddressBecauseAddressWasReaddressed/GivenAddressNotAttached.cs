namespace ParcelRegistry.Tests.AggregateTests.WhenReplacingAttachedAddressBecauseAddressWasReaddressed
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Extensions;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
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
        public void WithPreviousAddressAttached_ThenParcelAddressWasReplacedBecauseAddressWasReaddressed()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReplaceAttachedAddressBecauseAddressWasReaddressed(
                Fixture.Create<ParcelId>(),
                addressPersistentLocalId,
                previousAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    previousAddressPersistentLocalId,
                    new AddressPersistentLocalId(2)
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        addressPersistentLocalId,
                        previousAddressPersistentLocalId))));
        }

        [Fact]
        public void WithPreviousAddressNotAttached_ThenParcelAddressWasReplacedBecauseAddressWasReaddressed()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReplaceAttachedAddressBecauseAddressWasReaddressed(
                Fixture.Create<ParcelId>(),
                addressPersistentLocalId,
                previousAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                command.ParcelId,
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    new AddressPersistentLocalId(2)
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                        command.ParcelId,
                        new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                        addressPersistentLocalId,
                        previousAddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<ParcelRegistry.Legacy.ParcelId>(),
                Fixture.Create<ParcelId>(),
                Fixture.Create<VbrCaPaKey>(),
                ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId>
                {
                    previousAddressPersistentLocalId,
                    otherAddressPersistentLocalId,
                },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry());
            ((ISetProvenance)parcelWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var attachedParcelAddressWasReplacedBecauseAddressWasReaddressed = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                Fixture.Create<ParcelId>(),
                new VbrCaPaKey(parcelWasMigrated.CaPaKey),
                addressPersistentLocalId,
                previousAddressPersistentLocalId);
            ((ISetProvenance)attachedParcelAddressWasReplacedBecauseAddressWasReaddressed).SetProvenance(Fixture.Create<Provenance>());

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, attachedParcelAddressWasReplacedBecauseAddressWasReaddressed });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.AddressPersistentLocalIds.Should().NotContain(previousAddressPersistentLocalId);
            sut.LastEventHash.Should().Be(attachedParcelAddressWasReplacedBecauseAddressWasReaddressed.GetHash());
        }
    }
}
