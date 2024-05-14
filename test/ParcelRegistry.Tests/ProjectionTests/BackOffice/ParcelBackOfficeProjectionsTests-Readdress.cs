namespace ParcelRegistry.Tests.ProjectionTests.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using FluentAssertions;
    using Parcel.Events;
    using Xunit;

    public partial class ParcelBackOfficeProjectionsTests
    {
        [Fact]
        public async Task GivenOnlyPreviousParcelAddressRelationExistsWithCountOne_ThenRelationIsReplaced()
        {
            var @event = _fixture.Create<ParcelAddressWasReplacedBecauseAddressWasReaddressed>();

            await AddRelation(@event.ParcelId, @event.PreviousAddressPersistentLocalId);

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var previousParcelAddressRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousParcelAddressRelation.Should().BeNull();

                    var newAddressParcelRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newAddressParcelRelation.Should().NotBeNull();
                    newAddressParcelRelation!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenPreviousParcelAddressRelationExistsWithCountTwo_ThenCountIsDecrementedByOne()
        {
            var @event = _fixture.Create<ParcelAddressWasReplacedBecauseAddressWasReaddressed>();

            var relation = new ParcelAddressRelation(@event.ParcelId, @event.PreviousAddressPersistentLocalId)
            {
                Count = 2
            };
            await _backOfficeContext.ParcelAddressRelations.AddAsync(relation, CancellationToken.None);
            await _backOfficeContext.SaveChangesAsync(CancellationToken.None);

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var previousParcelAddressRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousParcelAddressRelation.Should().NotBeNull();
                    previousParcelAddressRelation!.Count.Should().Be(1);

                    var newAddressParcelRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newAddressParcelRelation.Should().NotBeNull();
                    newAddressParcelRelation!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenNewParcelAddressRelationsExists_ThenCountIsIncrementedByOne()
        {
            var @event = _fixture.Create<ParcelAddressWasReplacedBecauseAddressWasReaddressed>();

            await AddRelation(@event.ParcelId, @event.PreviousAddressPersistentLocalId);
            await AddRelation(@event.ParcelId, @event.NewAddressPersistentLocalId);

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var previousParcelAddressRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousParcelAddressRelation.Should().BeNull();

                    var newAddressParcelRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newAddressParcelRelation.Should().NotBeNull();
                    newAddressParcelRelation!.Count.Should().Be(2);
                });
        }
    }
}
