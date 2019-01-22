namespace ParcelRegistry.Api.Legacy.Infrastructure
{
    using System.Reflection;
    using System.Threading;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Be.Vlaanderen.Basisregisters.Api;

    [ApiVersionNeutral]
    [Route("")]
    public class EmptyController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get(
            [FromServices] IHostingEnvironment hostingEnvironment,
            CancellationToken cancellationToken)
            => Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? (IActionResult)new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the Basisregisters Vlaanderen Parcel Api v{Assembly.GetEntryAssembly().GetName().Version}.");
    }
}
