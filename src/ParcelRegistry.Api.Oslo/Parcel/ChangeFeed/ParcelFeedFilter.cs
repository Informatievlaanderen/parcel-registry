namespace ParcelRegistry.Api.Oslo.Parcel.ChangeFeed
{
    public sealed class ParcelFeedFilter
    {
        public int? Page { get; set; }
        public long? FeedPosition { get; set; }
    }
}
