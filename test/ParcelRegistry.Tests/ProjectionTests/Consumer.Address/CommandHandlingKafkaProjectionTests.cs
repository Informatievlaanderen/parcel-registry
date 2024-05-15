namespace ParcelRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Linq;
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
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext([]);
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

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
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

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
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

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
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

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Never);
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Never);
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Never);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRemoved()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRemovedV2(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRemovedBecauseHouseNumberWasRemoved(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRejected()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRejected(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseHouseNumberWasRejected()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRejected(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecause_AddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRejected(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecause_AddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRetired(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecause_AddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRejectedBecauseStreetNameWasRetired(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRetiredV2()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRetiredV2(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseHouseNumberWasRetired()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRetiredBecauseHouseNumberWasRetired(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseStreetNameWasRejected()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRetiredBecauseStreetNameWasRejected(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseStreetNameWasRetired()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRetiredBecauseStreetNameWasRetired(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseStreetNameWasRemoved()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRemovedBecauseStreetNameWasRemoved(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        // [Fact]
        // public async Task StreetNameWasReaddressed()
        // {
        //     var sourceAddressPersistentLocalId = 1;
        //     var sourceBoxNumberAddressPersistentLocalId = 2;
        //     var destinationAddressPersistentLocalId = 3;
        //     var destinationBoxNumberAddressPersistentLocalId = 4;
        //
        //     var @event = new AddressHouseNumberWasReaddressed(
        //         1000000,
        //         sourceAddressPersistentLocalId,
        //         new ReaddressedAddressData(
        //             sourceAddressPersistentLocalId,
        //             destinationAddressPersistentLocalId,
        //             true,
        //             "Current",
        //             "120",
        //             null,
        //             "9000",
        //             "AppointedByAdministrator",
        //             "Entry",
        //             "ExtendedWkbGeometry",
        //             true),
        //         new[]
        //         {
        //             new ReaddressedAddressData(
        //                 sourceBoxNumberAddressPersistentLocalId,
        //                 destinationBoxNumberAddressPersistentLocalId,
        //                 true,
        //                 "Current",
        //                 "120",
        //                 "A",
        //                 "9000",
        //                 "AppointedByAdministrator",
        //                 "Entry",
        //                 "ExtendedWkbGeometry",
        //                 true),
        //         },
        //         new Provenance(
        //             Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
        //             Application.ParcelRegistry.ToString(),
        //             Modification.Update.ToString(),
        //             Organisation.Aiv.ToString(),
        //             "test"));
        //
        //     AddParcelAddressRelations(sourceAddressPersistentLocalId);
        //     AddParcelAddressRelations(sourceBoxNumberAddressPersistentLocalId);
        //
        //     Given(@event);
        //     await Then(async _ =>
        //     {
        //         _mockCommandHandler.Verify(x =>
        //                 x.Handle(It.IsAny<ReplaceAttachedAddressBecauseAddressWasReaddressed>(), CancellationToken.None),
        //             Times.Exactly(2));
        //
        //         _mockCommandHandler.Verify(x =>
        //                 x.Handle(
        //                     It.Is<ReplaceAttachedAddressBecauseAddressWasReaddressed>(y =>
        //                         y.NewAddressPersistentLocalId == destinationAddressPersistentLocalId
        //                         && y.PreviousAddressPersistentLocalId == sourceAddressPersistentLocalId),
        //                     CancellationToken.None),
        //             Times.Exactly(1));
        //         _mockCommandHandler.Verify(x =>
        //                 x.Handle(
        //                     It.Is<ReplaceAttachedAddressBecauseAddressWasReaddressed>(y =>
        //                         y.NewAddressPersistentLocalId == destinationBoxNumberAddressPersistentLocalId
        //                         && y.PreviousAddressPersistentLocalId == sourceBoxNumberAddressPersistentLocalId),
        //                     CancellationToken.None),
        //             Times.Exactly(1));
        //
        //         await Task.CompletedTask;
        //     });
        // }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRejectedBecauseOfReaddress()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRejectedBecauseOfReaddress(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressBecauseAddressWasRetiredBecauseOfReaddress()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasRetiredBecauseOfReaddress(
                123,
                addressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [addressPersistentLocalId]);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                    x.Handle(It.IsAny<DetachAddressBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task AttachAndDetachAddressesWhenStreetNameWasReaddressed()
        {
            var parcelOneId = Fixture.Create<ParcelId>();
            var parcelTwoId = Fixture.Create<ParcelId>();

            var sourceAddressPersistentLocalIdOne = 1;
            var sourceAddressPersistentLocalIdTwo = 2;
            var sourceAddressPersistentLocalIdThree = 5;
            var unattachedSourceAddressPersistentLocalIdOne = 20;
            var destinationAddressPersistentLocalIdOne = 10;
            var destinationAddressPersistentLocalIdTwo = 11;
            var destinationAddressPersistentLocalIdThree = 12;

            var parcelOneAddressPersistentLocalIds = new[] { sourceAddressPersistentLocalIdOne, sourceAddressPersistentLocalIdTwo, 3 };
            var parcelTwoAddressPersistentLocalIds = new[] { 4, sourceAddressPersistentLocalIdThree };

            AddParcelAddressRelations(parcelOneId, parcelOneAddressPersistentLocalIds);
            AddParcelAddressRelations(parcelTwoId, parcelTwoAddressPersistentLocalIds);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [6, 7, 8]);

            var @event = new StreetNameWasReaddressed(
                Fixture.Create<int>(),
                new[]
                {
                    new AddressHouseNumberReaddressedData(
                        destinationAddressPersistentLocalIdOne,
                        CreateReaddressedAddressData(sourceAddressPersistentLocalIdOne, destinationAddressPersistentLocalIdOne),
                        new[]
                        {
                            CreateReaddressedAddressData(sourceAddressPersistentLocalIdTwo, destinationAddressPersistentLocalIdTwo),
                            CreateReaddressedAddressData(unattachedSourceAddressPersistentLocalIdOne, 21),
                        }),
                    new AddressHouseNumberReaddressedData(
                        destinationAddressPersistentLocalIdThree,
                        CreateReaddressedAddressData(sourceAddressPersistentLocalIdThree, destinationAddressPersistentLocalIdThree),
                        Array.Empty<ReaddressedAddressData>()),
                },
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test")
            );

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                        x.Handle(
                            It.Is<ReaddressAddresses>(y =>
                                y.ParcelId == parcelOneId
                                && y.Readdresses.Count == 2
                                && y.Readdresses.Any(z =>
                                    z.SourceAddressPersistentLocalId == sourceAddressPersistentLocalIdOne
                                    && z.DestinationAddressPersistentLocalId == destinationAddressPersistentLocalIdOne)
                                && y.Readdresses.Any(z => z.SourceAddressPersistentLocalId == sourceAddressPersistentLocalIdTwo
                                                          && z.DestinationAddressPersistentLocalId == destinationAddressPersistentLocalIdTwo)),
                            CancellationToken.None),
                    Times.Once);
                _mockCommandHandler.Verify(x =>
                        x.Handle(
                            It.Is<ReaddressAddresses>(y =>
                                y.ParcelId == parcelTwoId
                                && y.Readdresses.Count == 1
                                && y.Readdresses.Any(z =>
                                    z.SourceAddressPersistentLocalId == sourceAddressPersistentLocalIdThree
                                    && z.DestinationAddressPersistentLocalId == destinationAddressPersistentLocalIdThree)),
                            CancellationToken.None),
                    Times.Once);
                await Task.CompletedTask;
            });
        }

        private ReaddressedAddressData CreateReaddressedAddressData(
            int sourceAddressPersistentLocalIdOne,
            int destinationAddressPersistentLocalIdOne)
        {
            return new ReaddressedAddressData(
                sourceAddressPersistentLocalIdOne,
                destinationAddressPersistentLocalIdOne,
                Fixture.Create<bool>(),
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                Fixture.Create<string>(),
                Fixture.Create<bool>());
        }

        private void AddParcelAddressRelations(ParcelId parcelId, int[] addressPersistentLocalIds)
        {
            foreach (var addressPersistentLocalId in addressPersistentLocalIds)
            {
                _fakeBackOfficeContext.ParcelAddressRelations.Add(
                    new ParcelAddressRelation(parcelId, new AddressPersistentLocalId(addressPersistentLocalId)));
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
