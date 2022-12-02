namespace ParcelRegistry.Tests.BackOffice.Api.WhenDetachingAddress
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using FluentAssertions;
    using Moq;
    using ParcelRegistry.Api.BackOffice;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Validators;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInvalidIfMatchHeader : BackOfficeApiTest
    {
        private readonly ParcelController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenInvalidIfMatchHeader(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _streamStore = new Mock<IStreamStore>();
            _controller = CreateParcelControllerWithUser();
        }

        [Fact]
        public async Task ThenPreconditionFailedResponse()
        {
            _streamStore.SetStreamFound();

            //Act
            var result = await _controller.DetachAddress(
                MockValidRequestValidator<DetachAddressRequest>(),
                new ParcelExistsValidator(_streamStore.Object),
                MockIfMatchValidator(false),
                Fixture.Create<string>(),
                Fixture.Create<DetachAddressRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
