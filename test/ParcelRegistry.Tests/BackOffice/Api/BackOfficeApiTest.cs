namespace ParcelRegistry.Tests.BackOffice.Api
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Parcel;
    using ParcelRegistry.Api.BackOffice;
    using ParcelRegistry.Api.BackOffice.Infrastructure;
    using ParcelRegistry.Api.BackOffice.Infrastructure.Options;
    using Xunit.Abstractions;

    public class BackOfficeApiTest : ParcelRegistryTest
    {
        private const string PublicTicketUrl = "https://www.ticketing.com";
        private const string InternalTicketUrl = "https://www.internalticketing.com";

        private IOptions<TicketingOptions> TicketingOptions { get; }
        protected Mock<IMediator> MockMediator { get; }

        private const string Username = "John Doe";

        protected BackOfficeApiTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            TicketingOptions = Options.Create(Fixture.Create<TicketingOptions>());
            TicketingOptions.Value.PublicBaseUrl = PublicTicketUrl;
            TicketingOptions.Value.InternalBaseUrl = InternalTicketUrl;

            MockMediator = new Mock<IMediator>();
        }

        protected IIfMatchHeaderValidator MockIfMatchValidator(bool expectedResult)
        {
            var ifMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();
            ifMatchHeaderValidator
                .Setup(x => x.IsValid(It.IsAny<string>(), It.IsAny<ParcelId>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedResult));

            return ifMatchHeaderValidator.Object;
        }

        protected IValidator<TRequest> MockValidRequestValidator<TRequest>()
        {
            var mockRequestValidator = new Mock<IValidator<TRequest>>();

            mockRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));

            return mockRequestValidator.Object;
        }

        protected Uri CreateTicketUri(Guid ticketId)
        {
            return new Uri($"{InternalTicketUrl}/tickets/{ticketId:D}");
        }

        protected void AssertLocation(string? location, Guid ticketId)
        {
            var expectedLocation = $"{PublicTicketUrl}/tickets/{ticketId:D}";

            location.Should().NotBeNullOrWhiteSpace();
            location.Should().Be(expectedLocation);
        }

        protected ParcelController CreateParcelControllerWithUser()
        {
            if (Activator.CreateInstance(typeof(ParcelController), MockMediator.Object, TicketingOptions) is not ParcelController controller)
            {
                throw new Exception("Could not find controller type");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "username"),
                new(ClaimTypes.NameIdentifier, "userId"),
                new("name", Username),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            return controller;

        }
    }
}
