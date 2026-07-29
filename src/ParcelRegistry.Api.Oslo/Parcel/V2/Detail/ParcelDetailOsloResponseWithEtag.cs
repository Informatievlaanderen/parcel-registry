namespace ParcelRegistry.Api.Oslo.Parcel.V2.Detail
{
    public class ParcelDetailOsloResponseWithEtag
    {
        public ParcelDetailOsloResponse ParcelResponse { get; }
        public string? LastEventHash { get; }

        public ParcelDetailOsloResponseWithEtag(ParcelDetailOsloResponse parcelResponse, string? lastEventHash = null)
        {
            ParcelResponse = parcelResponse;
            LastEventHash = lastEventHash;
        }
    }
}
