namespace ParcelRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public partial class CommandHandlingKafkaProjectionTests
    {
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

            var parcelOneExpectedAddressPersistentLocalIds =
                new[] { destinationAddressPersistentLocalIdOne, destinationAddressPersistentLocalIdTwo, 3 };
            var parcelTwoExpectedAddressPersistentLocalIds = new[] { 4, destinationAddressPersistentLocalIdThree };

            // Setup BackofficeContext
            AddParcelAddressRelations(parcelOneId, parcelOneAddressPersistentLocalIds);
            AddParcelAddressRelations(parcelTwoId, parcelTwoAddressPersistentLocalIds);
            AddParcelAddressRelations(Fixture.Create<ParcelId>(), [6, 7, 8]);

            // Setup domain
            SetupParcelWithAddresses(parcelOneId, parcelOneExpectedAddressPersistentLocalIds);
            SetupParcelWithAddresses(parcelTwoId, parcelTwoExpectedAddressPersistentLocalIds);

            // Act
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
                            CreateReaddressedAddressData(unattachedSourceAddressPersistentLocalIdOne, 21)
                        }),
                    new AddressHouseNumberReaddressedData(
                        destinationAddressPersistentLocalIdThree,
                        CreateReaddressedAddressData(sourceAddressPersistentLocalIdThree, destinationAddressPersistentLocalIdThree),
                        [])
                },
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test")
            );

            Given(@event);

            // Assert
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x =>
                        x.HandleIdempotent(
                            It.Is<ReaddressAddresses>(y =>
                                y.ParcelId == parcelOneId
                                && y.Readdresses.Count == 2
                                && y.Readdresses.Any(z =>
                                    z.SourceAddressPersistentLocalId == sourceAddressPersistentLocalIdOne
                                    && z.DestinationAddressPersistentLocalId == destinationAddressPersistentLocalIdOne)
                                && y.Readdresses.Any(z =>
                                    z.SourceAddressPersistentLocalId == sourceAddressPersistentLocalIdTwo
                                    && z.DestinationAddressPersistentLocalId == destinationAddressPersistentLocalIdTwo)
                                && y.Provenance.Timestamp.ToString() == @event.Provenance.Timestamp
                            ),
                            CancellationToken.None),
                    Times.Once);
                _mockCommandHandler.Verify(x =>
                        x.HandleIdempotent(
                            It.Is<ReaddressAddresses>(y =>
                                y.ParcelId == parcelTwoId
                                && y.Readdresses.Count == 1
                                && y.Readdresses.Any(z =>
                                    z.SourceAddressPersistentLocalId == sourceAddressPersistentLocalIdThree
                                    && z.DestinationAddressPersistentLocalId == destinationAddressPersistentLocalIdThree)
                                && y.Provenance.Timestamp.ToString() == @event.Provenance.Timestamp
                            ),
                            CancellationToken.None),
                    Times.Once);

                _mockCommandHandler.Invocations.Count.Should().Be(2);

                var parcelOneRelations = _fakeBackOfficeContext.ParcelAddressRelations
                    .Where(x => x.ParcelId == parcelOneId)
                    .ToList();
                parcelOneRelations.Count.Should().Be(parcelOneExpectedAddressPersistentLocalIds.Length);
                foreach (var addressPersistentLocalId in parcelOneExpectedAddressPersistentLocalIds)
                {
                    var expectedRelation =
                        parcelOneRelations.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);
                    expectedRelation.Should().NotBeNull();
                    expectedRelation!.Count.Should().Be(1);
                }

                var parcelTwoRelations = _fakeBackOfficeContext.ParcelAddressRelations
                    .Where(x => x.ParcelId == parcelTwoId)
                    .ToList();
                parcelTwoRelations.Count.Should().Be(parcelTwoExpectedAddressPersistentLocalIds.Length);
                foreach (var addressPersistentLocalId in parcelTwoExpectedAddressPersistentLocalIds)
                {
                    var expectedRelation =
                        parcelTwoRelations.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);
                    expectedRelation.Should().NotBeNull();
                    expectedRelation!.Count.Should().Be(1);
                }

                await Task.CompletedTask;
            });
        }

        private void SetupParcelWithAddresses(ParcelId parcelId, IEnumerable<int> addressPersistentLocalIds)
        {
            var parcel = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
            var events = addressPersistentLocalIds
                .Select(addressPersistentLocalId =>
                {
                    var parcelAddressWasAttached = new ParcelAddressWasAttachedV2(
                        parcelId, Fixture.Create<VbrCaPaKey>(), new AddressPersistentLocalId(addressPersistentLocalId));
                    parcelAddressWasAttached.SetFixtureProvenance(Fixture);
                    return parcelAddressWasAttached;
                })
                .ToList();
            parcel.Initialize(events);

            _parcels
                .Setup(x => x.GetAsync(new ParcelStreamId(parcelId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parcel);
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
    }
}
