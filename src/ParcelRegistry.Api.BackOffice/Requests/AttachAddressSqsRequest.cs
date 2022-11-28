namespace ParcelRegistry.Api.BackOffice.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Parcel;

    public class AttachAddressSqsRequest : SqsRequest
    {
        public ParcelId ParcelId { get; set; }
        public AttachAddressRequest Request { get; set; }
    }
}
