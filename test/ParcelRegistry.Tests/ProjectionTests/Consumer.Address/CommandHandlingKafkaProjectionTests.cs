namespace ParcelRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;
    using ParcelRegistry.Consumer.Address;
    using ParcelRegistry.Consumer.Address.Projections;
    using Tests.BackOffice;
    using Xunit;
    using Xunit.Abstractions;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public sealed class CommandHandlingKafkaProjectionTests : KafkaProjectionTest<CommandHandler, CommandHandlingKafkaProjection>
    {
        private readonly FakeBackOfficeContext _fakeBackOfficeContext;
        private readonly Mock<FakeCommandHandler> _mockCommandHandler;

        public CommandHandlingKafkaProjectionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());

            _mockCommandHandler = new Mock<FakeCommandHandler>();
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task DetachAddressBecauseRemovedAddressWasMigrated()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: string.Empty,
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId,addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseRejectedAddressWasMigrated()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: AddressStatus.Rejected,
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId, addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseRetiredAddressWasMigrated()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: AddressStatus.Retired,
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId, addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("Current")]
        [InlineData("Proposed")]
        public async Task DoNothingWhenAddressStatus(string status)
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: AddressStatus.Parse(status),
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId, addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None),
                    Times.Never);
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None),
                    Times.Never);
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None),
                    Times.Never);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRemoved()
        {
            var addressIntId = 456;

            var @event = new AddressWasRemovedV2(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
             await Then(async _ =>
                {
                    _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                    await Task.CompletedTask;
                });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressIntId = 456;

            var @event = new AddressWasRemovedBecauseHouseNumberWasRemoved(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
             await Then(async _ =>
                {
                    _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                    await Task.CompletedTask;
                });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseHouseNumberWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecause_AddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecause_AddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecause_AddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseStreetNameWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRetiredV2()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredV2(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseHouseNumberWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseHouseNumberWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseStreetNameWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseStreetNameWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseStreetNameWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseStreetNameWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        private void AddRelations(params int[] addressInts)
        {
            foreach (var addressInt in addressInts)
            {
                _fakeBackOfficeContext.ParcelAddressRelations.Add(
                    new ParcelAddressRelation(Fixture.Create<ParcelId>(),
                        new AddressPersistentLocalId(addressInt)));
            }

            _fakeBackOfficeContext.SaveChanges();
        }

        protected override CommandHandler CreateContext()
        {
            return _mockCommandHandler.Object;
        }

        protected override CommandHandlingKafkaProjection CreateProjection()
        {
            var factoryMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            factoryMock
                .Setup(x => x.CreateDbContextAsync(CancellationToken.None))
                .Returns(Task.FromResult<BackOfficeContext>(_fakeBackOfficeContext));
            return new CommandHandlingKafkaProjection(factoryMock.Object);
        }
    }

    public class FakeCommandHandler : CommandHandler
    {
        public FakeCommandHandler() : base(null, new NullLoggerFactory())
        { }
    }
}
