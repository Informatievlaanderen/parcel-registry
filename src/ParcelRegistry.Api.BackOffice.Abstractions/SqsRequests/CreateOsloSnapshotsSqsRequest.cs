namespace ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public class CreateOsloSnapshotsSqsRequest : SqsRequest
    {
        public CreateOsloSnapshotsRequest Request { get; set; }
    }
}
