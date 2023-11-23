namespace ParcelRegistry.Tests.AggregateTests.WhenAttachingParcelAddress
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Consumer.Address;
    using Fixtures;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Parcel;
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
        public void ThenParcelAddressWasAttachedV2()
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(111);

            var command = new AttachAddressBuilder(Fixture)
                .WithAddress(addressPersistentLocalId)
                .Build();

            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Current,
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            Assert(new Scenario()
                .Given(
                    new ParcelStreamId(command.ParcelId),
                    parcelWasMigrated)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                    new ParcelAddressWasAttachedV2(command.ParcelId, new VbrCaPaKey(parcelWasMigrated.CaPaKey), command.AddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var parcelWasMigrated = new ParcelWasMigratedBuilder(Fixture)
                .WithStatus(ParcelStatus.Realized)
                .WithAddress(123)
                .Build();

            var addressPersistentLocalId = new AddressPersistentLocalId(111);
            var parcelAddressWasAttachedV2 = new ParcelAddressWasAttachedV2(Fixture.Create<ParcelId>(), new VbrCaPaKey(parcelWasMigrated.CaPaKey), addressPersistentLocalId);
            ((ISetProvenance)parcelAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act
            var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            sut.Initialize(new List<object> { parcelWasMigrated, parcelAddressWasAttachedV2 });

            // Assert
            sut.AddressPersistentLocalIds.Should().HaveCount(2);
            sut.AddressPersistentLocalIds.Should().Contain(addressPersistentLocalId);
            sut.LastEventHash.Should().Be(parcelAddressWasAttachedV2.GetHash());
        }
    }
}
