namespace ParcelRegistry.Api.Extract.Extracts
{
    using System;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Converters;
    using Projections.Extract;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extract")]
    [ApiExplorerSettings(GroupName = "Extract")]
    public class ExtractController : ApiController
    {
        public const string ZipName = "percelen";

        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als perceelregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelRegistryResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public IActionResult Get(
            [FromServices] ExtractContext context,
            CancellationToken cancellationToken = default) =>
            new ExtractArchive($"{ZipName}-{DateTime.Now:yyyy-MM-dd}") { ParcelRegistryExtractBuilder.CreateParcelFile(context) }
                .CreateFileCallbackResult(cancellationToken);
    }
}
