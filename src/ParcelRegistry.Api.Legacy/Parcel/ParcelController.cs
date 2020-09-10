namespace ParcelRegistry.Api.Legacy.Parcel
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Newtonsoft.Json.Converters;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("percelen")]
    [ApiExplorerSettings(GroupName = "Percelen")]
    public class ParcelController : ApiController
    {
        /// <summary>
        /// Vraag een perceel op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ParcelNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(ParcelGoneResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] string caPaKey,
            CancellationToken cancellationToken = default)
        {
            var parcel =
                await context
                    .ParcelDetail
                    .Include(x => x.Addresses)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.PersistentLocalId == caPaKey, cancellationToken);

            if (parcel != null && parcel.Removed)
                throw new ApiException("Perceel werd verwijderd.", StatusCodes.Status410Gone);

            if (parcel == null || !parcel.Complete)
                throw new ApiException("Onbestaand perceel.", StatusCodes.Status404NotFound);

            var addressIds = parcel.Addresses.Select(x => x.AddressId);

            var addressPersistentLocalIds = await syndicationContext
                .AddressPersistentLocalIds
                .AsNoTracking()
                .Where(x => addressIds.Contains(x.AddressId) && x.IsComplete && !x.IsRemoved)
                .Select(x => x.PersistentLocalId)
                .OrderBy(x => x) //sorts on string! other order as a number!
                .ToListAsync(cancellationToken);

            return Ok(
                new ParcelResponse(
                    responseOptions.Value.Naamruimte,
                    parcel.Status.MapToPerceelStatus(),
                    parcel.PersistentLocalId,
                    parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                    addressPersistentLocalIds.ToList(),
                    responseOptions.Value.AdresDetailUrl));
        }

        /// <summary>
        /// Vraag een lijst met actieve percelen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reponseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met percelen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ParcelListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> reponseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<ParcelFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedParcels = new ParcelListQuery(context)
                .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedParcels);

            var response = new ParcelListResponse
            {
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemResponse(
                        m.PersistentLocalId,
                        reponseOptions.Value.Naamruimte,
                        reponseOptions.Value.DetailUrl,
                        m.VersionTimestamp.ToBelgianDateTimeOffset()))
                    .ToListAsync(cancellationToken),
                Volgende = BuildVolgendeUri(pagedParcels.PaginationInfo, reponseOptions.Value.VolgendeUrl)
            };

            return Ok(response);
        }

        /// <summary>
        /// Vraag het totaal aantal actieve percelen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Count(
            [FromServices] LegacyContext context,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<ParcelFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new ParcelListQuery(context)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32(context
                            .ParcelDetailListViewCount
                            .First()
                            .Count)
                });
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van percelen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<ParcelSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var lastFeedUpdate = await context
                .ParcelSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var pagedParcels = new ParcelSyndicationQuery(
                context,
                filtering.Filter?.Embed)
                .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedParcels);

            return new ContentResult
            {
                Content = await BuildAtomFeed(lastFeedUpdate, pagedParcels, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        private static async Task<string> BuildAtomFeed(
            DateTimeOffset lastUpdate,
            PagedQueryable<ParcelSyndicationQueryResult> pagedParcels,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");
                var atomConfiguration = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastUpdate);

                await writer.WriteDefaultMetadata(atomConfiguration);

                var nextUri = BuildVolgendeUri(pagedParcels.PaginationInfo, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, "next"));

                foreach (var parcel in pagedParcels.Items)
                    await writer.WriteParcel(responseOptions, formatter, syndicationConfiguration["Category"], parcel);

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri BuildVolgendeUri(PaginationInfo paginationInfo, string volgendeUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage
                ? new Uri(string.Format(volgendeUrlBase, offset + limit, limit))
                : null;
        }
    }
}
