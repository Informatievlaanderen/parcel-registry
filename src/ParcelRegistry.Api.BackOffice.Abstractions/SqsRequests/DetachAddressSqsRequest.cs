namespace ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Parcel;

    public class DetachAddressSqsRequest : SqsRequest
    {
        public ParcelId ParcelId { get; set; }
        public VbrCaPaKey VbrCaPaKey { get; set; }
        public DetachAddressRequest Request { get; set; }
    }
}
