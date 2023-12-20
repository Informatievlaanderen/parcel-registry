namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Extensions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Parcel;
    using Parcel.Commands;

    public sealed record AttachAddressLambdaRequest : ParcelLambdaRequest, IHasParcelId
    {
        public AttachAddressRequest Request { get; }

        public Guid ParcelId => ParcelRegistry.Parcel.ParcelId.CreateFor(new VbrCaPaKey(VbrCaPaKey));
        public string VbrCaPaKey { get; }

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
            VbrCaPaKey = sqsRequest.VbrCaPaKey;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to AttachAddress command
        /// </summary>
        /// <returns>AttachAddress.</returns>
        public AttachAddress ToCommand()
        {
            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(Request.AdresId);

            return new AttachAddress(
                new ParcelId(ParcelId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                CommandProvenance);
        }
    }
}
