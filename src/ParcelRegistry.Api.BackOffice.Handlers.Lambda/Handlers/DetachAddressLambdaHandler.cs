namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Parcel;
    using Parcel.Exceptions;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class DetachAddressLambdaHandler : ParcelLambdaHandler<DetachAddressLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public DetachAddressLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IParcels parcels,
            BackOfficeContext backOfficeContext)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                parcels)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<ETagResponse> InnerHandle(DetachAddressLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            _backOfficeContext.ParcelAddressRelations.Remove(new ParcelAddressRelation(cmd.ParcelId, cmd.AddressPersistentLocalId));
            await _backOfficeContext.SaveChangesAsync(cancellationToken);

            var lastHash = await Parcels.GetHash(new ParcelId(request.ParcelId), cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.VbrCaPaKey), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, DetachAddressLambdaRequest request)
        {
            return exception switch
            {
                AddressNotFoundException => ValidationErrors.Common.AdresIdInvalid.ToTicketError,
                AddressIsRemovedException => ValidationErrors.Common.AddressRemoved.ToTicketError,
                _ => null
            };
        }
    }
}
