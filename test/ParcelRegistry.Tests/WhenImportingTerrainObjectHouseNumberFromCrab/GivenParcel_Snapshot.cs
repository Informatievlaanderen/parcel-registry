namespace ParcelRegistry.Tests.WhenImportingTerrainObjectHouseNumberFromCrab
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using Parcel.Commands.Crab;
    using Parcel.Events;
    using SnapshotTests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcel_Snapshot : ParcelRegistrySnapshotTest
    {
        private readonly ParcelId _parcelId;
        private readonly string _snapshotId;
        private readonly Fixture _fixture;

        public GivenParcel_Snapshot(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new WithNoDeleteModification());
            _parcelId = _fixture.Create<ParcelId>();
            _snapshotId = GetSnapshotIdentifier(_parcelId);
        }

        [Fact]
        public void WhenDeleteAndInfiniteLifetimeWithAddress_WithSnapshot()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Insert);

            var deleteCommand = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(command.TerrainObjectHouseNumberId)
                .WithHouseNumberId(command.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var addressId = AddressId.CreateFor(command.HouseNumberId);

            Assert(new Scenario()
                .Given(_parcelId,
                    _fixture.Create<ParcelWasRegistered>(),
                    _fixture.Create<ParcelAddressWasAttached>()
                        .WithAddressId(addressId),
                    command.ToLegacyEvent())
                .When(deleteCommand)
                .Then(new[]
                {
                    new Fact(_parcelId, new ParcelAddressWasDetached(_parcelId, addressId)),
                    new Fact(_parcelId, deleteCommand.ToLegacyEvent()),
                    new Fact(_snapshotId,
                        SnapshotBuilder.CreateDefaultSnapshot(_parcelId)
                            .WithLastModificationBasedOnCrab(Modification.Update)
                            .Build(4, EventSerializerSettings))
                }));
        }
    }
}
