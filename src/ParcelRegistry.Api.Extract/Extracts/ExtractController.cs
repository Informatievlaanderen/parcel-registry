namespace ParcelRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using ExtractFiles;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Converters;
    using Projections.Extract;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extract")]
    [ApiExplorerSettings(GroupName = "Extract")]
    public class ExtractController : ApiController
    {
        public static string ZipName = "percelen";

        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als perceelregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelRegistryResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] ExtractContext context,
            CancellationToken cancellationToken = default)
        {
            var parcels = await context
                .ParcelExtract
                .AsNoTracking()
                .OrderBy(m => m.CaPaKey)
                .ToListAsync(cancellationToken);

            var zip = new List<ExtractFile>
            {
                ParcelRegistryExtractBuilder.CreateParcelFile(parcels)
            };

            return zip.CreateResponse($"{ZipName}-{DateTime.Now:yyyy-MM-dd}");
        }
    }
}
