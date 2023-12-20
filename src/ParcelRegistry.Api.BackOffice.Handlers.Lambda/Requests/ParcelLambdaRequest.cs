namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using NodaTime;

    public abstract record ParcelLambdaRequest : SqsLambdaRequest
    {
        protected ParcelLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        { }

        protected Provenance CommandProvenance => new Provenance(
            SystemClock.Instance.GetCurrentInstant(),
            Provenance.Application,
            Provenance.Reason,
            Provenance.Operator,
            Provenance.Modification,
            Provenance.Organisation);
    }
}
