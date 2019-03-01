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
    using Microsoft.AspNetCore.Hosting;
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
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
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
                    .SingleOrDefaultAsync(item => item.OsloId == caPaKey, cancellationToken);

            if (parcel == null || !parcel.Complete)
                throw new ApiException("Onbestaand perceel.", StatusCodes.Status404NotFound);

            if (parcel.Removed)
                throw new ApiException("Perceel werd verwijderd.", StatusCodes.Status410Gone);

            var addressIds = parcel.Addresses.Select(x => x.AddressId);

            var addressOlsoIdItems = await syndicationContext
                .AddressOsloIds
                .AsNoTracking()
                .Where(x => addressIds.Contains(x.AddressId) && x.IsComplete && !x.IsRemoved)
                .Select(x => x.OsloId)
                .ToListAsync(cancellationToken);

            return Ok(
                new ParcelResponse(
                    responseOptions.Value.Naamruimte,
                    parcel.Status.MapToPerceelStatus(),
                    parcel.OsloId,
                    parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                    addressOlsoIdItems.ToList(),
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
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
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

            var pagedParcels = new ParcelListQuery(context).Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedParcels.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            var response = new ParcelListResponse
            {
                Percelen = await pagedParcels.Items
                    .Select(m => new ParcelListItemResponse(
                        m.OsloId,
                        reponseOptions.Value.Naamruimte,
                        reponseOptions.Value.DetailUrl,
                        m.VersionTimestamp.ToBelgianDateTimeOffset()))
                    .ToListAsync(cancellationToken),
                TotaalAantal = pagedParcels.PaginationInfo.TotalItems,
                Volgende = BuildVolgendeUri(pagedParcels.PaginationInfo, reponseOptions.Value.VolgendeUrl)
            };

            return Ok(response);
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
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
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

            var pagedAddresses = new ParcelSyndicationQuery(context).Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedAddresses.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            return new ContentResult
            {
                Content = await BuildAtomFeed(pagedAddresses, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        private static async Task<string> BuildAtomFeed(
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

                await writer.WriteDefaultMetadata(
                    syndicationConfiguration["Id"],
                    syndicationConfiguration["Title"],
                    Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    new Uri(syndicationConfiguration["Self"]),
                    syndicationConfiguration.GetSection("Related").GetChildren().Select(c => c.Value).ToArray());

                var nextUri = BuildVolgendeUri(pagedParcels.PaginationInfo, syndicationConfiguration["NextUri"]);
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

            return offset + limit < paginationInfo.TotalItems
                ? new Uri(string.Format(volgendeUrlBase, offset + limit, limit))
                : null;
        }
    }
}
