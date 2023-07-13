namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    using System;

    public sealed class OrderDetailResponse
    {
        public OrderDownload[] Downloads { get; set; }

        public int OrderId { get; set; }
        public DateTime OrderTimestamp { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime StatusTimestamp { get; set; }
    }
}
