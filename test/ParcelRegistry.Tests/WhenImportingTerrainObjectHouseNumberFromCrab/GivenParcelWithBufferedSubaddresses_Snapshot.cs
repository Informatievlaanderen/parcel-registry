namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using Parcel.Events.Crab;
    using SnapshotTests;
    using WhenImportingSubaddressFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelWithBufferedSubaddresses_Snapshot : ParcelRegistrySnapshotTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcelWithBufferedSubaddresses_Snapshot(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WithNoDeleteAndInfiniteLifetime_WithSnapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var subaddress1 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            var subaddress2 = _fixture.Create<ImportSubaddressFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    subaddress1.ToLegacyEvent(),
                    subaddress2.ToLegacyEvent())
                .When(command)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(command.HouseNumberId))),
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress1.SubaddressId))),
                    new Fact(_parcelId, new ParcelAddressWasAttached(_parcelId, AddressId.CreateFor(subaddress2.SubaddressId))),
                    new Fact(_parcelId, command.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .WithImportedSubaddressFromCrab(new List<AddressSubaddressWasImportedFromCrab>
                            {
                                subaddress1.ToLegacyEvent(),
                                subaddress2.ToLegacyEvent()
                            })
                            .WithAddressIds(new List<AddressId>
                            {
                                AddressId.CreateFor(command.HouseNumberId),
                                AddressId.CreateFor(subaddress1.SubaddressId),
                                AddressId.CreateFor(subaddress2.SubaddressId)
                            })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { new CrabTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId), new CrabHouseNumberId(command.HouseNumberId) }
                            })
                            .Build(6, EventSerializerSettings))
                }));
        }
    }
}
