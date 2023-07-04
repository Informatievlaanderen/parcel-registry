namespace ParcelRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Consumer.Address;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Exceptions;
    using ParcelRegistry.Api.BackOffice.Abstractions;
    using ParcelRegistry.Api.BackOffice.Abstractions.Extensions;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;
    using Coordinate = Parcel.Coordinate;

    public class WhenDetachingAddressRequest : LambdaHandlerTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;

        public WhenDetachingAddressRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithValidVbrCaPaKey());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
            Fixture.Customize(new Legacy.AutoFixture.WithParcelStatus());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var etag = string.Empty;
            var ticketing = MockTicketing(response => { etag = response.ETag; });

            var vbrCaPaKey = Fixture.Create<VbrCaPaKey>();
            var legacyParcelId = ParcelRegistry.Legacy.ParcelId.CreateFor(vbrCaPaKey);
            var parcelId = ParcelId.CreateFor(vbrCaPaKey);
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Current,
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            await _backOfficeContext.ParcelAddressRelations.AddAsync(new ParcelAddressRelation((Guid)parcelId, (int)addressPersistentLocalId));
            await _backOfficeContext.SaveChangesAsync();
            _backOfficeContext.ChangeTracker.Clear();

            DispatchArrangeCommand(new MigrateParcel(
                legacyParcelId,
                vbrCaPaKey,
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId> { addressPersistentLocalId, new AddressPersistentLocalId(456) },
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>()));

            var handler = new DetachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IParcels>(),
                _backOfficeContext);

         // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new DetachAddressLambdaRequestBuilder(Fixture)
                    .WithVbrCaPaKey(vbrCaPaKey)
                    .WithAdresId(addressPersistentLocalId)
                    .WithTicketId(ticketId)
                    .Build(),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, vbrCaPaKey),
                            etag)),
                    CancellationToken.None));

            var addressParcelRelation = _backOfficeContext.ParcelAddressRelations
                .FirstOrDefault(x => x.ParcelId == (Guid)parcelId && x.AddressPersistentLocalId == (int)addressPersistentLocalId);
            addressParcelRelation.Should().BeNull();
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var parcels = new Mock<IParcels>();
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var vbrCaPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId =  ParcelId.CreateFor(vbrCaPaKey);
            const string expectedEventHash = "lastEventHash";

            parcels
                .Setup(x => x.GetHash(parcelId, CancellationToken.None))
                .ReturnsAsync(() => expectedEventHash);

            await _backOfficeContext.ParcelAddressRelations.AddAsync(new ParcelAddressRelation((Guid)parcelId, (int)addressPersistentLocalId));
            await _backOfficeContext.SaveChangesAsync();
            _backOfficeContext.ChangeTracker.Clear();

            var handler = new DetachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                parcels.Object,
                _backOfficeContext);

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new DetachAddressLambdaRequestBuilder(Fixture)
                    .WithVbrCaPaKey(vbrCaPaKey)
                    .WithAdresId(addressPersistentLocalId)
                    .WithTicketId(ticketId)
                    .Build(),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, vbrCaPaKey),
                            expectedEventHash)),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBackOfficeRelationsIsAlreadyRemoved_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var etag = string.Empty;
            var ticketing = MockTicketing(response => { etag = response.ETag; });

            var vbrCaPaKey = Fixture.Create<VbrCaPaKey>();
            var legacyParcelId = ParcelRegistry.Legacy.ParcelId.CreateFor(vbrCaPaKey);
            var parcelId = ParcelId.CreateFor(vbrCaPaKey);
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(
                addressPersistentLocalId,
                AddressStatus.Current,
                "DerivedFromObject",
                "Parcel",
                (Point)_wkbReader.Read(Fixture.Create<ExtendedWkbGeometry>().ToString().ToByteArray()));

            DispatchArrangeCommand(new MigrateParcel(
                legacyParcelId,
                vbrCaPaKey,
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId> { addressPersistentLocalId },
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                Fixture.Create<Provenance>()));

            var handler = new DetachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IParcels>(),
                _backOfficeContext);

         // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new DetachAddressLambdaRequestBuilder(Fixture)
                    .WithVbrCaPaKey(vbrCaPaKey)
                    .WithAdresId(addressPersistentLocalId)
                    .WithTicketId(ticketId)
                    .Build(),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, vbrCaPaKey),
                            etag)),
                    CancellationToken.None));

            var addressParcelRelation = _backOfficeContext.ParcelAddressRelations
                .FirstOrDefault(x => x.ParcelId == (Guid)parcelId && x.AddressPersistentLocalId == (int)addressPersistentLocalId);
            addressParcelRelation.Should().BeNull();
        }

        [Fact]
        public async Task WhenParcelIsRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var handler = new DetachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<ParcelIsRemovedException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new DetachAddressLambdaRequestBuilder(Fixture)
                    .WithAdresId(addressPersistentLocalId)
                    .Build(),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Verwijderd perceel.",
                        "VerwijderdPerceel"),
                    CancellationToken.None));
        }
    }
}
