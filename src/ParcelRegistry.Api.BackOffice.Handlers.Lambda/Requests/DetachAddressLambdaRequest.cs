namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Parcel;
    using Parcel.Commands;
    using Abstractions.Extensions;

    public sealed record DetachAddressLambdaRequest : ParcelLambdaRequest, IHasParcelId
    {
        public DetachAddressRequest Request { get; }

        public Guid ParcelId => ParcelRegistry.Parcel.ParcelId.CreateFor(new VbrCaPaKey(VbrCaPaKey));
        public string VbrCaPaKey { get; }

        public DetachAddressLambdaRequest(
            string messageGroupId,
            DetachAddressSqsRequest sqsRequest)
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
        /// Map to DetachAddress command
        /// </summary>
        /// <returns>DetachAddress.</returns>
        public DetachAddress ToCommand()
        {
            var addressPersistentLocalId = OsloPuriValidatorExtensions.ParsePersistentLocalId(Request.AdresId);

            return new DetachAddress(
                new ParcelId(ParcelId),
                new AddressPersistentLocalId(addressPersistentLocalId),
                CommandProvenance);
        }
    }
}
