namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    public sealed class OrderRequest
    {
        public int ProductId { get; set; }
        public string Format { get; set; }
        public TemporalCrop TemporalCrop { get; set; }
    }
}
