namespace ParcelRegistry.Api.Legacy.Parcel
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Converters;
    using Projections.Legacy;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;

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

            return Ok(
                new ParcelResponse(
                    responseOptions.Value.Naamruimte,
                    parcel.Status.MapToPerceelStatus(),
                    parcel.OsloId,
                    parcel.VersionTimestamp.ToBelgianDateTimeOffset(),
                    parcel.Addresses.Select(x => x.AddressId.ToString("D")).ToList(), // TODO: create view that joins legacy and syndication tables
                    responseOptions.Value.AdresDetailUrl));
        }

        /// <summary>
        /// Vraag een lijst met actieve percelen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="reponseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met percelen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ParcelListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ParcelListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] IHostingEnvironment hostingEnvironment,
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
