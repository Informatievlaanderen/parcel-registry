namespace ParcelRegistry.Api.Legacy.Parcel.Detail
{
    public class ParcelResponseWithEtag
    {
        public ParcelDetailResponse ParcelResponse { get; }
        public string? LastEventHash { get; }

        public ParcelResponseWithEtag(ParcelDetailResponse parcelResponse, string? lastEventHash = null)
        {
            ParcelResponse = parcelResponse;
            LastEventHash = lastEventHash;
        }
    }
}
