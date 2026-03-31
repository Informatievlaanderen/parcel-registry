namespace ParcelRegistry.Api.Oslo.Infrastructure.Options
{
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;

    public class ResponseOptions
    {
        public string Naamruimte { get; set; }
        public string VolgendeUrl { get; set; }
        public string DetailUrl { get; set; }
        public string AdresDetailUrl { get; set; }
        public string ContextUrlList { get; set; }
        public string ContextUrlDetail { get; set; }
        public string ParcelDetailBuildingsLink { get; set; }
        public ChangeFeedConfig ParcelFeed { get; set; }
    }
}
