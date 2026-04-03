namespace ParcelRegistry.Api.Oslo.Parcel
{
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using ChangeFeed;
    using CloudNative.CloudEvents;
    using Count;
    using Detail;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Projections.Feed;
    using Projections.Legacy;
    using Swashbuckle.AspNetCore.Filters;
    using Sync;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
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
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(ParcelDetailOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ParcelNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(ParcelGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] string caPaKey,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new ParcelDetailOsloRequest(caPaKey), cancellationToken);

            return string.IsNullOrWhiteSpace(response.LastEventHash)
                ? Ok(response.ParcelResponse)
                : new OkWithLastObservedPositionAsETagResult(response.ParcelResponse, response.LastEventHash);
        }

        /// <summary>
        /// Vraag een lijst met actieve percelen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met percelen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(ParcelListOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelListOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<ParcelFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var result = await _mediator.Send(new ParcelListOsloRequest(filtering, sorting, pagination), cancellationToken);

            Response.AddPaginationResponse(result.Pagination);
            Response.AddSortingResponse(result.Sorting);

            return Ok(result);
        }

        /// <summary>
        /// Vraag het totaal aantal actieve percelen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountOsloResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<ParcelFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return Ok(await _mediator.Send(new ParcelCountOsloRequest(filtering, sorting, pagination), cancellationToken));
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
                Content = await _mediator.Send(new SyncRequest(Request), cancellationToken),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van percelen op (CloudEvents).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="page"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Changes(
            [FromServices] FeedContext context,
            [FromQuery] int? page,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<ParcelFeedFilter>();
            if (page is null)
                page = filtering.Filter?.Page ?? 1;

            var feedPosition = filtering.Filter?.FeedPosition;

            if (feedPosition.HasValue && filtering.Filter?.Page.HasValue == false)
            {
                page = context.ParcelFeed
                    .Where(x => x.Position == feedPosition.Value)
                    .Select(x => x.Page)
                    .Distinct()
                    .AsEnumerable()
                    .DefaultIfEmpty(1)
                    .Min();
            }

            var feedItemsEvents = await context
                .ParcelFeed
                .Where(x => x.Page == page)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return new ChangeFeedResult(jsonContent, feedItemsEvents.Count >= ChangeFeedService.DefaultMaxPageSize);
        }

        /// <summary>
        /// Vraag wijzigingen van een bepaald perceel op (CloudEvents).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="caPaKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{caPaKey}/wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangesByCaPaKey(
            [FromServices] FeedContext context,
            [FromRoute] string caPaKey,
            CancellationToken cancellationToken = default)
        {
            var pagination = (PaginationRequest)Request.ExtractPaginationRequest();

            var feedItemsEvents = await context
                .ParcelFeed
                .Where(x => x.CaPaKey == caPaKey)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .Skip(pagination.Offset)
                .Take(pagination.Limit)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return Content(jsonContent, AcceptTypes.JsonCloudEventsBatch);
        }

        [HttpGet("posities")]
        [Produces(AcceptTypes.Json)]
        [ProducesResponseType(typeof(FeedPositieResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPositions(
            [FromServices] LegacyContext legacyContext,
            [FromServices] FeedContext feedContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressPositionFilter>();
            var response = new FeedPositieResponse();
            if (filtering.ShouldFilter && !filtering.Filter.HasMoreThanOneFilter)
            {
                if (filtering.Filter.Download.HasValue)
                {
                    var businessFeedPosition = await legacyContext
                        .ParcelSyndication
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    var changeFeed = await feedContext
                        .ParcelFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = businessFeedPosition;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.Sync.HasValue)
                {
                    var position = await legacyContext
                        .ParcelSyndication
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Sync.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    var changeFeed = await feedContext
                        .ParcelFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= position)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = filtering.Filter.Sync.Value;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.ChangeFeedId.HasValue)
                {
                    var feedItem = await feedContext
                        .ParcelFeed
                        .AsNoTracking()
                        .Where(x => x.Id == filtering.Filter.ChangeFeedId.Value)
                        .Select(x => new { x.Id, x.Page, x.Position })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (feedItem is null)
                        return Ok(response);

                    var syncPosition = await legacyContext
                        .ParcelSyndication
                        .AsNoTracking()
                        .Where(x => x.Position == feedItem.Position)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = syncPosition;
                    response.WijzigingenFeedPagina = feedItem.Page;
                    response.WijzigingenFeedId = feedItem.Id;
                }
            }

            return Ok(response);
        }
    }
}
