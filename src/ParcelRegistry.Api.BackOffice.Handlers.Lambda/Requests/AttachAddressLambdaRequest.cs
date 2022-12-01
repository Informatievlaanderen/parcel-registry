namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Parcel;
    using Parcel.Commands;

    public sealed record AttachAddressLambdaRequest : SqsLambdaRequest, IHasParcelId
    {
        public AttachAddressRequest Request { get; set; }

        public Guid ParcelId { get; }

        public AttachAddressLambdaRequest(
            string messageGroupId,
            AttachAddressSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            ParcelId = sqsRequest.ParcelId;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to AttachAddress command
        /// </summary>
        /// <returns>AttachAddress.</returns>
        public AttachAddress ToCommand()
        {
            return new AttachAddress(new ParcelId(ParcelId), new AddressPersistentLocalId(Request.AddressPersistentLocalId), Provenance);
        }
    }
}
