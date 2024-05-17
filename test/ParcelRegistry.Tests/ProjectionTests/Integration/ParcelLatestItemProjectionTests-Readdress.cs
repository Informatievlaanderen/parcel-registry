namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Builders;
    using Fixtures;
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

        [Fact]
        public async Task GivenParcelAddressesWereReaddressed_ThenAddressesAreAttachedAndDetached()
        {
            _fixture.Customizations.Add(new WithUniqueInteger());

            var firstParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var secondParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            var attachedAddressPersistentLocalIds = new[]
            {
                _fixture.Create<int>(),
                _fixture.Create<int>()
            };
            var detachedAddressPersistentLocalIds = new[]
            {
                firstParcelAddressWasAttachedV2.AddressPersistentLocalId,
                secondParcelAddressWasAttachedV2.AddressPersistentLocalId
            };

            var eventBuilder = new ParcelAddressesWereReaddressedBuilder(_fixture);

            foreach (var addressPersistentLocalId in attachedAddressPersistentLocalIds)
            {
                eventBuilder.WithAttachedAddress(addressPersistentLocalId);
            }

            foreach (var addressPersistentLocalId in detachedAddressPersistentLocalIds)
            {
                eventBuilder.WithDetachedAddress(addressPersistentLocalId);
            }

            var @event = eventBuilder.Build();

            await Sut
                .Given(
                    _fixture.Create<ParcelWasImported>(),
                    firstParcelAddressWasAttachedV2,
                    secondParcelAddressWasAttachedV2,
                    @event)
                .Then(async context =>
                {
                    foreach (var addressPersistentLocalId in attachedAddressPersistentLocalIds)
                    {
                        var parcelAddressRelation = await context.ParcelLatestItemAddresses.FindAsync(
                            @event.ParcelId,
                            addressPersistentLocalId);

                        parcelAddressRelation.Should().NotBeNull();
                        parcelAddressRelation!.Count.Should().Be(1);
                    }

                    foreach (var addressPersistentLocalId in detachedAddressPersistentLocalIds)
                    {
                        var parcelAddressRelation = await context.ParcelLatestItemAddresses.FindAsync(
                            @event.ParcelId,
                            addressPersistentLocalId);

                        parcelAddressRelation.Should().BeNull();
                    }
                });
        }
    }
}
