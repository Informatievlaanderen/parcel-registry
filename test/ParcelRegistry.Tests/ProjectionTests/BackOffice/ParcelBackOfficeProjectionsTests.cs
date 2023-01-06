namespace ParcelRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Events;
    using Tests.BackOffice;
    using Xunit;

    public class ParcelBackOfficeProjectionsTests : ParcelBackOfficeProjectionsTest
    {
        private readonly Fixture _fixture;
        private readonly FakeBackOfficeContext _fakeBackOfficeContext;

        public ParcelBackOfficeProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());

            _fakeBackOfficeContext =
                new FakeBackOfficeContextFactory(dispose: false).CreateDbContext(Array.Empty<string>());
            BackOfficeContextMock
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fakeBackOfficeContext);
        }

        [Fact]
        public async Task GivenParcelAddressWasAttachedV_ThenRelationIsAdded()
        {
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            await Sut
                .Given(parcelAddressWasAttachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasAttachedV2.ParcelId, parcelAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.ParcelId.Should().Be(parcelAddressWasAttachedV2.ParcelId);
                    result!.AddressPersistentLocalId.Should().Be(parcelAddressWasAttachedV2.AddressPersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasAttachedVAndRelationPresent_ThenNothing()
        {
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            var expectedRelation = await _fakeBackOfficeContext.AddIdempotentParcelAddressRelation(
                new ParcelId(parcelAddressWasAttachedV2.ParcelId),
                new AddressPersistentLocalId(parcelAddressWasAttachedV2.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(parcelAddressWasAttachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasAttachedV2.ParcelId, parcelAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedV2_ThenRelationIsRemovedAdded()
        {
            var parcelAddressWasDetachedV2 = _fixture.Create<ParcelAddressWasDetachedV2>();

            await _fakeBackOfficeContext.AddIdempotentParcelAddressRelation(
                new ParcelId(parcelAddressWasDetachedV2.ParcelId),
                new AddressPersistentLocalId(parcelAddressWasDetachedV2.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(parcelAddressWasDetachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedV2.ParcelId, parcelAddressWasDetachedV2.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedV2AndRelationDoesntExist_ThenNothing()
        {
            var parcelAddressWasDetachedV2 = _fixture.Create<ParcelAddressWasDetachedV2>();

            await Sut
                .Given(parcelAddressWasDetachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedV2.ParcelId, parcelAddressWasDetachedV2.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRejected_ThenRelationIsRemovedAdded()
        {
            var parcelAddressWasDetachedBecauseAddressWasRejected = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRejected>();

            await _fakeBackOfficeContext.AddIdempotentParcelAddressRelation(
                new ParcelId(parcelAddressWasDetachedBecauseAddressWasRejected.ParcelId),
                new AddressPersistentLocalId(parcelAddressWasDetachedBecauseAddressWasRejected.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRejected)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRejected.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRejected.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRejectedAndRelationDoesntExist_ThenNothing()
        {
            var parcelAddressWasDetachedBecauseAddressWasRejected = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRejected>();

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRejected)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRejected.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRejected.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRetired_ThenRelationIsRemovedAdded()
        {
            var parcelAddressWasDetachedBecauseAddressWasRetired = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRetired>();

            await _fakeBackOfficeContext.AddIdempotentParcelAddressRelation(
                new ParcelId(parcelAddressWasDetachedBecauseAddressWasRetired.ParcelId),
                new AddressPersistentLocalId(parcelAddressWasDetachedBecauseAddressWasRetired.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRetired)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRetired.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRetired.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRetiredAndRelationDoesntExist_ThenNothing()
        {
            var parcelAddressWasDetachedBecauseAddressWasRetired = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRetired>();

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRetired)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRetired.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRetired.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRemoved_ThenRelationIsRemovedAdded()
        {
            var parcelAddressWasDetachedBecauseAddressWasRemoved = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRemoved>();

            await _fakeBackOfficeContext.AddIdempotentParcelAddressRelation(
                new ParcelId(parcelAddressWasDetachedBecauseAddressWasRemoved.ParcelId),
                new AddressPersistentLocalId(parcelAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRemoved)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRemoved.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRemovedAndRelationDoesntExist_ThenNothing()
        {
            var parcelAddressWasDetachedBecauseAddressWasRemoved = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRemoved>();

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRemoved)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRemoved.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }
    }
}
