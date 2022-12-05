namespace ParcelRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Parcel;
    using Swashbuckle.AspNetCore.Filters;
    using Validators;

    public partial class ParcelController
    {
        /// <summary>
        /// Ontkoppel adres van perceel.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="parcelExistsValidator"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="caPaKey"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{caPaKey}/acties/adresontkoppelen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> DetachAddress(
            [FromServices] IValidator<DetachAddressRequest> validator,
            [FromServices] ParcelExistsValidator parcelExistsValidator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] string caPaKey,
            [FromBody] DetachAddressRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            var vbrCaPaKey = new VbrCaPaKey(caPaKey);
            var parcelId = ParcelId.CreateFor(vbrCaPaKey);

            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                if (!await parcelExistsValidator.Exists(parcelId, cancellationToken))
                {
                    throw new ApiException(ValidationErrors.Common.ParcelNotFound.Message, StatusCodes.Status404NotFound);
                }

                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, parcelId, cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                var sqsRequest = new DetachAddressSqsRequest
                {
                    ParcelId = parcelId,
                    VbrCaPaKey = vbrCaPaKey,
                    Request = request,
                    IfMatchHeaderValue = ifMatchHeaderValue,
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                };

                var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                return Accepted(sqsResult);
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.ParcelNotFound.Message, StatusCodes.Status404NotFound);
            }
        }
    }
}
