namespace ParcelRegistry.Tests.BackOffice.Api.WhenAttachingAddress
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using ParcelRegistry.Api.BackOffice;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Requests;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Validators;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRequest : BackOfficeApiTest
    {
        private readonly ParcelController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _streamStore = new Mock<IStreamStore>();
            _controller = CreateParcelControllerWithUser();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var caPaKey = Guid.NewGuid().ToString("D");
            var parcelId = ParcelId.CreateFor(new VbrCaPaKey(caPaKey));

            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.Is<AttachAddressSqsRequest>(request => request.ParcelId == parcelId), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var result = (AcceptedResult)await _controller.AttachAddress(
                MockValidRequestValidator<AttachAddressRequest>(),
                new ParcelExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                caPaKey,
                Fixture.Create<AttachAddressRequest>(),
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }
    }
}
