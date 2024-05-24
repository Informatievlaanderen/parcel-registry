namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using AllStream.Commands;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;

    public sealed record CreateOsloSnapshotsLambdaRequest : SqsLambdaRequest
    {
        public CreateOsloSnapshotsRequest Request { get; }


        public CreateOsloSnapshotsLambdaRequest(
            string messageGroupId,
            CreateOsloSnapshotsSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CreateOsloSnapshots command
        /// </summary>
        /// <returns>CreateOsloSnapshots</returns>
        public CreateOsloSnapshots ToCommand()
        {
            return new CreateOsloSnapshots(
                Request.CaPaKeys.Select(x => new VbrCaPaKey(x)),
                Provenance);
        }
    }
}
