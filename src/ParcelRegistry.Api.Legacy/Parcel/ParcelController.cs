namespace ParcelRegistry.Api.Legacy.Parcel
{
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Requests;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("percelen")]
    [ApiExplorerSettings(GroupName = "Percelen")]
    public class ParcelController : ApiController
    {
        private readonly IMediator _mediator;

        public ParcelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Vraag een perceel op.
        /// </summary>
        /// <param name="caPaKey">Identificator van het perceel.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het perceel gevonden is.</response>
        /// <response code="404">Als het perceel niet gevonden kan worden.</response>
        /// <response code="410">Als het perceel verwijderd is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{caPaKey}")]
        [ProducesResponseType(typeof(ParcelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ParcelNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(ParcelGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] string caPaKey,
            CancellationToken cancellationToken = default)
        {
            return Ok(await _mediator.Send(new GetParcelRequest(caPaKey), cancellationToken));
        }

        /// <summary>
        /// Vraag een lijst met actieve percelen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met percelen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ParcelListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelListResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            CancellationToken cancellationToken = default)
        {
            return Ok(await _mediator.Send(new GetParcelListRequest(Request, Response), cancellationToken));
        }

        /// <summary>
        /// Vraag het totaal aantal actieve percelen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
        {
            return Ok(await _mediator.Send(new GetCountRequest(Request), cancellationToken));
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van percelen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Sync(CancellationToken cancellationToken = default)
        {
            return new ContentResult
            {
                Content = await _mediator.Send(new GetSyncRequest(Request), cancellationToken),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
