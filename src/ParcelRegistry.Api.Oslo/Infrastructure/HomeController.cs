namespace ParcelRegistry.Api.Oslo.Infrastructure
{
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Be.Vlaanderen.Basisregisters.Api;

    [ApiVersionNeutral]
    [Route("")]
    public class HomeController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Get()
            => Request.Headers[HeaderNames.Accept].ToString().Contains("text/html")
                ? (IActionResult)new RedirectResult("/docs")
                : new OkObjectResult($"Welcome to the Basisregisters Vlaanderen Parcel Oslo Api {Assembly.GetEntryAssembly().GetVersionText()}.");
    }
}
