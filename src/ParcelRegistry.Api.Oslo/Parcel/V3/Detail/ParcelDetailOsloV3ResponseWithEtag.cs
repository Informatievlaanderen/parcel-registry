namespace ParcelRegistry.Api.Oslo.Parcel.V3.Detail
{
    public class ParcelDetailOsloV3ResponseWithEtag
    {
        public ParcelDetailOsloV3Response ParcelResponse { get; }
        public string? LastEventHash { get; }

        public ParcelDetailOsloV3ResponseWithEtag(ParcelDetailOsloV3Response parcelResponse, string? lastEventHash = null)
        {
            ParcelResponse = parcelResponse;
            LastEventHash = lastEventHash;
        }
    }
}
