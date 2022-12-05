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

    public sealed class AttachAddressLambdaHandler : ParcelLambdaHandler<AttachAddressLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public AttachAddressLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(AttachAddressLambdaRequest request, CancellationToken cancellationToken)
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

            _backOfficeContext.ParcelAddressRelations.Add(new ParcelAddressRelation(cmd.ParcelId, cmd.AddressPersistentLocalId));
            await _backOfficeContext.SaveChangesAsync(cancellationToken);

            var lastHash = await Parcels.GetHash(new ParcelId(request.ParcelId), cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.ParcelId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, AttachAddressLambdaRequest request)
        {
            return exception switch
            {
                ParcelHasInvalidStatusException => ValidationErrors.AttachAddress.InvalidParcelStatus.ToTicketError,
                AddressNotFoundException => ValidationErrors.Common.AddressNotFound.ToTicketError,
                AddressIsRemovedException => ValidationErrors.Common.AddressRemoved.ToTicketError,
                AddressHasInvalidStatusException => ValidationErrors.AttachAddress.InvalidAddressStatus.ToTicketError,
                _ => null
            };
        }
    }
}
