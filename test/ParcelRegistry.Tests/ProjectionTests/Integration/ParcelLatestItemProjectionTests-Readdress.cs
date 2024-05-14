namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Parcel;
    using Parcel.Events;
    using Xunit;

    public partial class ParcelLatestItemProjectionTests
    {
        [Fact]
        public async Task GivenOnlyPreviousParcelAddressRelationExistsWithCountOne_ThenRelationIsReplaced()
        {
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<ParcelId>(),
                _fixture.Create<VbrCaPaKey>(),
                newAddressPersistentLocalId: new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId + 1),
                previousAddressPersistentLocalId: new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId));

            await Sut
                .Given(
                    _fixture.Create<ParcelWasImported>(),
                    parcelAddressWasAttachedV2,
                    @event)
                .Then(async context =>
                {
                    var previousRelation = await context.ParcelLatestItemAddresses.FindAsync(
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousRelation.Should().BeNull();

                    var newRelation = await context.ParcelLatestItemAddresses.FindAsync(
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newRelation.Should().NotBeNull();
                    newRelation!.CaPaKey.Should().Be(@event.CaPaKey);
                    newRelation.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenPreviousParcelAddressRelationExistsWithCountTwo_ThenCountIsDecrementedByOne()
        {
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var eventToAddPreviousRelationASecondTime = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<ParcelId>(),
                _fixture.Create<VbrCaPaKey>(),
                newAddressPersistentLocalId: new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId),
                previousAddressPersistentLocalId: new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId + 100));

            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<ParcelId>(),
                _fixture.Create<VbrCaPaKey>(),
                newAddressPersistentLocalId: new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId + 101),
                previousAddressPersistentLocalId: new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId));

            await Sut
                .Given(
                    _fixture.Create<ParcelWasImported>(),
                    parcelAddressWasAttachedV2,
                    eventToAddPreviousRelationASecondTime,
                    @event)
                .Then(async context =>
                {
                    var previousRelation = await context.ParcelLatestItemAddresses.FindAsync(
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousRelation.Should().NotBeNull();
                    previousRelation!.Count.Should().Be(1);

                    var newRelation = await context.ParcelLatestItemAddresses.FindAsync(
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newRelation.Should().NotBeNull();
                    newRelation!.CaPaKey.Should().Be(@event.CaPaKey);
                    newRelation.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenNewParcelAddressRelationsExists_ThenCountIsIncrementedByOne()
        {
            var previousParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var newParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<ParcelId>(),
                _fixture.Create<VbrCaPaKey>(),
                newAddressPersistentLocalId: new AddressPersistentLocalId(newParcelAddressWasAttachedV2.AddressPersistentLocalId),
                previousAddressPersistentLocalId: new AddressPersistentLocalId(previousParcelAddressWasAttachedV2.AddressPersistentLocalId));

            await Sut
                .Given(
                    _fixture.Create<ParcelWasImported>(),
                    previousParcelAddressWasAttachedV2,
                    newParcelAddressWasAttachedV2,
                    @event)
                .Then(async context =>
                {
                    var previousRelation = await context.ParcelLatestItemAddresses.FindAsync(
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousRelation.Should().BeNull();

                    var newRelation = await context.ParcelLatestItemAddresses.FindAsync(
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newRelation.Should().NotBeNull();
                    newRelation!.CaPaKey.Should().Be(@event.CaPaKey);
                    newRelation.Count.Should().Be(2);
                });
        }
    }
}
