namespace ParcelRegistry.Api.Legacy.Parcel.Responses
{
    public class ParcelResponseWithEtag
    {
        public ParcelResponse ParcelResponse { get; }
        public string? LastEventHash { get; }

        public ParcelResponseWithEtag(ParcelResponse parcelResponse, string? lastEventHash = null)
        {
            ParcelResponse = parcelResponse;
            LastEventHash = lastEventHash;
        }
    }
}
