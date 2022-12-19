namespace ParcelRegistry.Api.Oslo.Parcel.Responses
{
    public class ParcelOsloResponseWithEtag
    {
        public ParcelOsloResponse ParcelResponse { get; }
        public string? LastEventHash { get; }

        public ParcelOsloResponseWithEtag(ParcelOsloResponse parcelResponse, string? lastEventHash = null)
        {
            ParcelResponse = parcelResponse;
            LastEventHash = lastEventHash;
        }
    }
}
