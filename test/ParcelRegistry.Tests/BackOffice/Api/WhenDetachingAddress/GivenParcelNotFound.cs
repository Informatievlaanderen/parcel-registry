namespace ParcelRegistry.Tests.BackOffice.Api.WhenDetachingAddress
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using ParcelRegistry.Api.BackOffice;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Validators;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelNotFound : BackOfficeApiTest
    {
        private readonly Mock<IStreamStore> _streamStore;
        private readonly ParcelController _controller;

        public GivenParcelNotFound(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _streamStore = new Mock<IStreamStore>();
            _controller = CreateParcelControllerWithUser();
        }

        [Fact]
        public void ThenThrowApiException()
        {
            _streamStore.SetStreamNotFound();

            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    MockValidRequestValidator<DetachAddressRequest>(),
                    new ParcelExistsValidator(_streamStore.Object),
                    MockIfMatchValidator(true),
                    Fixture.Create<string>(),
                    Fixture.Create<DetachAddressRequest>(),
                    ifMatchHeaderValue: null);
            };

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaand perceel.");
        }
    }
}
