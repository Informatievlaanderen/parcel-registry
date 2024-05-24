namespace ParcelRegistry.Tests.BackOffice.Api.WhenRequestingCreateOsloSnapshots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using ParcelRegistry.Api.BackOffice;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRequest : BackOfficeApiTest
    {
        private readonly ParcelController _controller;

        public GivenRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithValidVbrCaPaKey());

            _controller = CreateParcelControllerWithUser();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var vbrCaPaKeys = Fixture.Create<List<VbrCaPaKey>>();

            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(
                    It.IsAny<CreateOsloSnapshotsSqsRequest>(),
                    CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = new CreateOsloSnapshotsRequest
            {
                CaPaKeys = vbrCaPaKeys.Select(x => (string)x).ToList(),
                Reden = "UnitTest"
            };

            var result = (AcceptedResult)await _controller.CreateOsloSnapshots(request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CreateOsloSnapshotsSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.ParcelRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Unknown
                        && sqsRequest.ProvenanceData.Reason == request.Reden
                    ),
                    CancellationToken.None));
        }
    }
}
