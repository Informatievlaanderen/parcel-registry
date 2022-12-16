namespace ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public class DetachAddressSqsRequest : SqsRequest
    {
        public string VbrCaPaKey { get; set; }
        public DetachAddressRequest Request { get; set; }
    }
}
