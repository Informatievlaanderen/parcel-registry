namespace ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public class AttachAddressSqsRequest : SqsRequest
    {
        public string VbrCaPaKey { get; set; }
        public AttachAddressRequest Request { get; set; }
    }
}
