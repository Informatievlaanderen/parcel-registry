namespace ParcelRegistry.Tests.BackOffice.Handler
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Moq;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using ParcelRegistry.Api.BackOffice.Handlers;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAttachAddressBackOfficeRequest : ParcelRegistryTest
    {
        public GivenAttachAddressBackOfficeRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithValidVbrCaPaKey());
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new AttachAddressHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl);

            var sqsRequest = new AttachAddressSqsRequest
            {
                VbrCaPaKey = Fixture.Create<VbrCaPaKey>(),
                Request = new AttachAddressRequest
                {
                    AdresId = PuriCreator.CreateAdresId(123)
                }
            };

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);

            ticketingMock.Verify(x => x.CreateTicket(new Dictionary<string, string>
            {
                {AttachAddressHandler.RegistryKey, nameof(ParcelRegistry)},
                { AttachAddressHandler.ActionKey, "AttachAddressParcel" },
                { AttachAddressHandler.AggregateIdKey, ParcelId.CreateFor(new VbrCaPaKey(sqsRequest.VbrCaPaKey)) },
                { AttachAddressHandler.ObjectIdKey, sqsRequest.VbrCaPaKey }
            }, CancellationToken.None));

            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == ParcelId.CreateFor(new VbrCaPaKey(sqsRequest.VbrCaPaKey)).ToString()),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }
    }
}
