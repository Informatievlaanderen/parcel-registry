namespace ParcelRegistry.Tests.BackOffice.Handler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using ParcelRegistry.Api.BackOffice.Handlers;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCreateOsloSnapshotsBackOfficeRequest : ParcelRegistryTest
    {
        public GivenCreateOsloSnapshotsBackOfficeRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

            var sut = new CreateOsloSnapshotsHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl);

            var sqsRequest = new CreateOsloSnapshotsSqsRequest
            {
                Request = new CreateOsloSnapshotsRequest
                {
                    CaPaKeys = Fixture.Create<List<VbrCaPaKey>>().Select(x => (string)x).ToList()
                }
            };

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);

            ticketingMock.Verify(x => x.CreateTicket(new Dictionary<string, string>
            {
                { AttachAddressHandler.RegistryKey, nameof(ParcelRegistry) },
                { AttachAddressHandler.ActionKey, "CreateOsloSnapshots" },
                { AttachAddressHandler.AggregateIdKey, AllStreamId.Instance },
            }, CancellationToken.None));

            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == AllStreamId.Instance.ToString()),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }
    }
}
