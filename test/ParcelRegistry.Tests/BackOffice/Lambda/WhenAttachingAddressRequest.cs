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
            Fixture.Customize(new WithFixedParcelId());
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

            var capakey = new VbrCaPaKey("capakey");
            var legacyParcelId = ParcelRegistry.Legacy.ParcelId.CreateFor(capakey);
            var parcelId = ParcelId.CreateFor(capakey);
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressPersistentLocalId, AddressStatus.Current);

            DispatchArrangeCommand(new MigrateParcel(
                legacyParcelId,
                capakey,
                ParcelRegistry.Legacy.ParcelStatus.Realized,
                isRemoved: false,
                Fixture.Create<IEnumerable<AddressPersistentLocalId>>(),
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
                    .WithParcelId(parcelId)
                    .WithAddressPersistentLocalId(addressPersistentLocalId)
                    .WithTicketId(ticketId)
                    .Build(),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, parcelId),
                            etag)),
                    CancellationToken.None));

            var addressParcelRelation = _backOfficeContext.ParcelAddressRelations.Find((Guid)parcelId, (int)addressPersistentLocalId);
            addressParcelRelation.Should().NotBeNull();
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
                new AttachAddressLambdaRequestBuilder(Fixture)
                    .WithParcelId(parcelId)
                    .WithTicketId(ticketId)
                    .Build(),
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
                new AttachAddressLambdaRequestBuilder(Fixture)
                    .WithAddressPersistentLocalId(addressPersistentLocalId)
                    .Build(),
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
                new AttachAddressLambdaRequestBuilder(Fixture)
                    .WithAddressPersistentLocalId(addressPersistentLocalId)
                    .Build(),
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
                new AttachAddressLambdaRequestBuilder(Fixture)
                    .WithAddressPersistentLocalId(addressPersistentLocalId)
                    .Build(),
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
                MockExceptionIdempotentCommandHandler<AddressHasInvalidStatusException>().Object,
                Container.Resolve<IParcels>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new AttachAddressLambdaRequestBuilder(Fixture)
                    .WithAddressPersistentLocalId(addressPersistentLocalId)
                    .Build(),
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
