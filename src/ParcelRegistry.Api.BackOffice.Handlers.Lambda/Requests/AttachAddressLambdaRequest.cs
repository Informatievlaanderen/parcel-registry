namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using BackOffice.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
            : this(
                messageGroupId,
                sqsRequest.ParcelId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public AttachAddressLambdaRequest(
            string messageGroupId,
            Guid parcelId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            AttachAddressRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            ParcelId = parcelId;
            Request = request;
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
