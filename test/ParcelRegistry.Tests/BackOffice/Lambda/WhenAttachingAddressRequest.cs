namespace ParcelRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
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
    using Builders;
    using Consumer.Address;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Exceptions;
    using ParcelRegistry.Api.BackOffice.Abstractions;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenAttachingAddressRequest : LambdaHandlerTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;

        public WhenAttachingAddressRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
            string etag = string.Empty;
            var ticketing = MockTicketing(response => { etag = response.ETag; });

            var vbrCaPaKey = Fixture.Create<VbrCaPaKey>();
            var legacyParcelId = ParcelRegistry.Legacy.ParcelId.CreateFor(vbrCaPaKey);
            var parcelId = ParcelId.CreateFor(vbrCaPaKey);

            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressPersistentLocalId, AddressStatus.Current);

            DispatchArrangeCommand(new MigrateParcel(
                legacyParcelId,
                vbrCaPaKey,
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                isRemoved: false,
                new List<AddressPersistentLocalId> { new AddressPersistentLocalId(456), new AddressPersistentLocalId(789) },
                Fixture.Create<Coordinate>(),
                Fixture.Create<Coordinate>(),
                Fixture.Create<Provenance>()));

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new AttachAddressLambdaRequestBuilder(Fixture)
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

            var addressParcelRelation = await _backOfficeContext.ParcelAddressRelations.FindAsync((Guid)parcelId, (int)addressPersistentLocalId);
            addressParcelRelation.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var parcels = new Mock<IParcels>();

            var vbrCaPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId =  ParcelId.CreateFor(vbrCaPaKey);
            const string expectedEventHash = "lastEventHash";

            parcels
                .Setup(x => x.GetHash(parcelId, CancellationToken.None))
                .ReturnsAsync(() => expectedEventHash);

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                parcels.Object,
                _backOfficeContext);

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new AttachAddressLambdaRequestBuilder(Fixture)
                    .WithVbrCaPaKey(vbrCaPaKey)
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
        public async Task WhenParcelHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<ParcelHasInvalidStatusException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(new AttachAddressLambdaRequestBuilder(Fixture).Build(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op percelen met status 'gerealiseerd'.",
                        "PerceelGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenParcelIsRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<ParcelIsRemovedException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(new AttachAddressLambdaRequestBuilder(Fixture).Build(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Verwijderd perceel.",
                        "VerwijderdPerceel"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressNotFound_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressNotFoundException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(new AttachAddressLambdaRequestBuilder(Fixture).Build(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig adresId.",
                        "PerceelAdresOngeldig"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressIsRemovedException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(new AttachAddressLambdaRequestBuilder(Fixture).Build(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Verwijderd adres.",
                        "VerwijderdAdres"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressHasInvalidStatusException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(new AttachAddressLambdaRequestBuilder(Fixture).Build(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Het adres is afgekeurd of gehistoreerd.",
                        "PerceelAdresAfgekeurdOfGehistoreerd"),
                    CancellationToken.None));
        }
    }
}
