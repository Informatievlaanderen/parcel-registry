namespace ParcelRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using Parcel.Events;
    using Tests.BackOffice;
    using Xunit;

    public partial class ParcelBackOfficeProjectionsTests : ParcelBackOfficeProjectionsTest
    {
        private readonly Fixture _fixture;
        private readonly FakeBackOfficeContext _backOfficeContext;

        public ParcelBackOfficeProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithParcelStatus());

            _backOfficeContext =
                new FakeBackOfficeContextFactory(dispose: false).CreateDbContext([]);
            BackOfficeContextMock
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_backOfficeContext);
        }

        [Fact]
        public async Task GivenParcelWasMigrated_ThenRelationsAreAdded()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();

            await Sut
                .Given(parcelWasMigrated)
                .Then(async _ =>
                {
                    foreach (var addressPersistentLocalId in parcelWasMigrated.AddressPersistentLocalIds)
                    {
                        var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                            parcelWasMigrated.ParcelId, addressPersistentLocalId);

                        result.Should().NotBeNull();
                        result!.ParcelId.Should().Be(parcelWasMigrated.ParcelId);
                        result.AddressPersistentLocalId.Should().Be(addressPersistentLocalId);
                    }
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasAttachedV2_ThenRelationIsAdded()
        {
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            await Sut
                .Given(parcelAddressWasAttachedV2)
                .Then(async _ =>
                {
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasAttachedV2.ParcelId, parcelAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.ParcelId.Should().Be(parcelAddressWasAttachedV2.ParcelId);
                    result.AddressPersistentLocalId.Should().Be(parcelAddressWasAttachedV2.AddressPersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasAttachedV2AndRelationPresent_ThenNothing()
        {
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            var expectedRelation = await AddRelation(
                parcelAddressWasAttachedV2.ParcelId,
                parcelAddressWasAttachedV2.AddressPersistentLocalId);

            await Sut
                .Given(parcelAddressWasAttachedV2)
                .Then(async _ =>
                {
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasAttachedV2.ParcelId, parcelAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedV2_ThenRelationIsRemoved()
        {
            var parcelAddressWasDetachedV2 = _fixture.Create<ParcelAddressWasDetachedV2>();

            await AddRelation(
                parcelAddressWasDetachedV2.ParcelId,
                parcelAddressWasDetachedV2.AddressPersistentLocalId);

            await Sut
                .Given(parcelAddressWasDetachedV2)
                .Then(async _ =>
                {
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
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
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedV2.ParcelId, parcelAddressWasDetachedV2.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRejected_ThenRelationIsRemoved()
        {
            var parcelAddressWasDetachedBecauseAddressWasRejected = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRejected>();

            await AddRelation(
                parcelAddressWasDetachedBecauseAddressWasRejected.ParcelId,
                parcelAddressWasDetachedBecauseAddressWasRejected.AddressPersistentLocalId);

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRejected)
                .Then(async _ =>
                {
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
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
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRejected.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRejected.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRetired_ThenRelationIsRemoved()
        {
            var parcelAddressWasDetachedBecauseAddressWasRetired = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRetired>();

            await AddRelation(
                parcelAddressWasDetachedBecauseAddressWasRetired.ParcelId,
                parcelAddressWasDetachedBecauseAddressWasRetired.AddressPersistentLocalId);

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRetired)
                .Then(async _ =>
                {
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
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
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRetired.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRetired.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRemoved_ThenRelationIsRemoved()
        {
            var parcelAddressWasDetachedBecauseAddressWasRemoved = _fixture.Create<ParcelAddressWasDetachedBecauseAddressWasRemoved>();

            await AddRelation(
                parcelAddressWasDetachedBecauseAddressWasRemoved.ParcelId,
                parcelAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId);

            await Sut
                .Given(parcelAddressWasDetachedBecauseAddressWasRemoved)
                .Then(async _ =>
                {
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
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
                    var result = await _backOfficeContext.ParcelAddressRelations.FindAsync(
                        parcelAddressWasDetachedBecauseAddressWasRemoved.ParcelId,
                        parcelAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        private async Task<ParcelAddressRelation> AddRelation(Guid parcelId, int addressPersistentLocalId)
        {
            return await _backOfficeContext.AddIdempotentParcelAddressRelation(
                new ParcelId(parcelId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                CancellationToken.None);
        }
    }
}
