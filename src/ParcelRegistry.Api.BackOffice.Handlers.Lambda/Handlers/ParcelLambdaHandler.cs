namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Configuration;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Microsoft.Extensions.Configuration;
    using Parcel;
    using Parcel.Exceptions;
    using TicketingService.Abstractions;
    using IHasParcelId = Requests.IHasParcelId;

    public abstract class ParcelLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaRequest
    {
        protected readonly IParcels Parcels;

        protected string DetailUrlFormat { get; }

        protected ParcelLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IParcels parcels)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
            Parcels = parcels;

            DetailUrlFormat = configuration["DetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'DetailUrl' cannot be found in the configuration");
            }
        }

        protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) ||
                request is not IHasParcelId id)
            {
                return;
            }

            var lastHash = await Parcels.GetHash(new ParcelId(id.ParcelId), cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            if (request.IfMatchHeaderValue != lastHashTag.ToString())
            {
                throw new IfMatchHeaderValueMismatchException();
            }
        }

        protected override async Task HandleAggregateIdIsNotFoundException(
            TSqsLambdaRequest request,
            CancellationToken cancellationToken)
        {
            await Ticketing.Error(
                request.TicketId,
                ValidationErrors.Common.ParcelNotFound.ToTicketError,
                cancellationToken);
        }

        protected abstract TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request);

        protected override TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request)
        {
            var error = InnerMapDomainException(exception, request);
            if (error is not null)
            {
                return error;
            }

            return exception switch
            {
                ParcelIsRemovedException => ValidationErrors.Common.ParcelRemoved.ToTicketError,
                _ => null
            };
        }
    }
}
