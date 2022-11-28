namespace ParcelRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Fixtures;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Parcel;
    using Parcel.Exceptions;
    using ParcelRegistry.Api.BackOffice.Abstractions;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenAttachingAddressRequest : LambdaHandlerTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;

        public WhenAttachingAddressRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
            Fixture.Customize(new Legacy.AutoFixture.WithParcelStatus());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var parcels = new Mock<IParcels>();

            var parcelId =  Fixture.Create<ParcelId>();
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
                new AttachAddressLambdaRequest(
                    messageGroupId:parcelId,
                    parcelId: parcelId,
                    ticketId: ticketId,
                    ifMatchHeaderValue: null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    Fixture.Create<AttachAddressRequest>()),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, parcelId),
                            expectedEventHash)),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenParcelHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<ParcelHasInvalidStatusException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new AttachAddressLambdaRequest(
                    messageGroupId: Guid.NewGuid().ToString(),
                    parcelId: Guid.NewGuid(),
                    ticketId: Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new AttachAddressRequest() { AddressPersistentLocalId = addressPersistentLocalId }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Enkel een gerealiseerd perceel kan gekoppeld worden.",
                        "PerceelGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressNotFound_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressNotFoundException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new AttachAddressLambdaRequest(
                    messageGroupId: Guid.NewGuid().ToString(),
                    parcelId: Guid.NewGuid(),
                    ticketId: Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new AttachAddressRequest() { AddressPersistentLocalId = addressPersistentLocalId }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig AdresId.",
                        "AdresOngeldig"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressIsRemovedException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new AttachAddressLambdaRequest(
                    messageGroupId: Guid.NewGuid().ToString(),
                    parcelId: Guid.NewGuid(),
                    ticketId: Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new AttachAddressRequest() { AddressPersistentLocalId = addressPersistentLocalId }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Verwijderd address.",
                        "VerwijderdAddress"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenAddressHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var handler = new AttachAddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<ParcelHasInvalidStatusException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new AttachAddressLambdaRequest(
                    messageGroupId: Guid.NewGuid().ToString(),
                    parcelId: Guid.NewGuid(),
                    ticketId: Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new AttachAddressRequest() { AddressPersistentLocalId = addressPersistentLocalId }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Enkel een voorgesteld of adres in gebruik kan gekoppeld worden.",
                        "AdresAfgekeurdGehistoreerd"),
                    CancellationToken.None));
        }
    }
}
