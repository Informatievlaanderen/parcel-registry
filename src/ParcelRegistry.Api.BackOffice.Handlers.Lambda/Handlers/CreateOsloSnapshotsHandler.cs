namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Requests;
    using TicketingService.Abstractions;

    public sealed class CreateOsloSnapshotsLambdaHandler : SqsLambdaHandlerBase<CreateOsloSnapshotsLambdaRequest>
    {
        public CreateOsloSnapshotsLambdaHandler(
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
        }

        protected override async Task<object> InnerHandle(CreateOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata!,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            return "done";
        }

        protected override TicketError? MapDomainException(DomainException exception, CreateOsloSnapshotsLambdaRequest request) => null;

        protected override Task HandleAggregateIdIsNotFoundException(CreateOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateIfMatchHeaderValue(CreateOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
