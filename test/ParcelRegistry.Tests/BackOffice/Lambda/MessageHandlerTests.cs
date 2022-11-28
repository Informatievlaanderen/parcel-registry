namespace ParcelRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using ParcelRegistry.Api.BackOffice.Requests;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class MessageHandlerTests : ParcelRegistryTest
    {
        public MessageHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task WhenProcessingAttachAddressSqsRequest_ThenAttachAddressLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<AttachAddressSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<AttachAddressLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.ParcelId == messageData.ParcelId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingUnknownMessage_ThenNothingIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<object>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<TestSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            var act = async () => await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }
    }

    internal class TestSqsRequest : SqsRequest
    { }
}
