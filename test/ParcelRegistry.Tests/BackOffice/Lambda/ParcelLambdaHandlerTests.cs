namespace ParcelRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Parcel;
    using Parcel.Exceptions;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class ParcelLambdaHandlerTests : LambdaHandlerTest
    {
        public ParcelLambdaHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task TicketShouldBeUpdatedToPendingAndCompleted()
        {
            var ticketing = new Mock<ITicketing>();
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var lambdaRequest = new AttachAddressLambdaRequest(
                messageGroupId: Guid.NewGuid().ToString(),
                parcelId: Guid.NewGuid(),
                ticketId: Guid.NewGuid(),
                null,
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new AttachAddressRequest() { AddressPersistentLocalId = 1 }
            );

            var sut = new FakeParcelLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IParcels>(),
                ticketing.Object,
                idempotentCommandHandler.Object);

            await sut.Handle(lambdaRequest, CancellationToken.None);

            ticketing.Verify(x => x.Pending(lambdaRequest.TicketId, CancellationToken.None), Times.Once);
            ticketing.Verify(
                x => x.Complete(lambdaRequest.TicketId,
                    new TicketResult(new ETagResponse("location", "etag")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenParcelIsRemovedException_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var lambdaRequest = new AttachAddressLambdaRequest(
                messageGroupId: Guid.NewGuid().ToString(),
                parcelId: Guid.NewGuid(),
                ticketId: Guid.NewGuid(),
                null,
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new AttachAddressRequest() { AddressPersistentLocalId = 1 }
            );

            var sut = new FakeParcelLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IParcels>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<ParcelIsRemovedException>().Object);

            await sut.Handle(lambdaRequest, CancellationToken.None);

            //Assert
            ticketing
                .Verify(x =>
                    x.Error(lambdaRequest.TicketId, new TicketError("Verwijderd perceel.", "VerwijderdPerceel"),
                        CancellationToken.None));
            ticketing
                .Verify(x =>
                    x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task WhenIfMatchHeaderValueIsMismatch_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var mockParcels = new Mock<IParcels>();
            mockParcels
                .Setup(x => x.GetHash(It.IsAny<ParcelId>(), CancellationToken.None))
                .ReturnsAsync("OtherHash");

            var sut = new FakeParcelLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                mockParcels.Object,
                ticketing.Object,
                Mock.Of<IIdempotentCommandHandler>());

            // Act
            await sut.Handle(
                new AttachAddressLambdaRequest(
                    addressPersistentLocalId.ToString(),
                    Guid.NewGuid(),
                    Guid.Empty,
                    "OutdatedHash",
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new AttachAddressRequest()
                    {
                        AddressPersistentLocalId = addressPersistentLocalId
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.",
                        "PreconditionFailed"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenNoIfMatchHeaderValueIsPresent_ThenInnerHandleIsCalled()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var sut = new FakeParcelLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IParcels>(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object);

            await sut.Handle(
                new AttachAddressLambdaRequest(
                    messageGroupId: Guid.NewGuid().ToString(),
                    parcelId: Guid.NewGuid(), 
                    ticketId: Guid.NewGuid(),
                    string.Empty,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new AttachAddressRequest() { AddressPersistentLocalId = 1 }),
                CancellationToken.None);

            //Assert
            idempotentCommandHandler
                .Verify(
                    x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                    Times.Once);
        }
    }

    public sealed class FakeParcelLambdaHandler : ParcelLambdaHandler<AttachAddressLambdaRequest>
    {
        public FakeParcelLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IParcels parcels,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                parcels)
        { }

        protected override Task<ETagResponse> InnerHandle(
            AttachAddressLambdaRequest request,
            CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult(new ETagResponse("location", "etag"));
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, AttachAddressLambdaRequest request)
        {
            return null;
        }
    }
}
