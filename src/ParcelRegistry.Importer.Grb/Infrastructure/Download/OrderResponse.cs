namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    using System.Text.Json.Serialization;

    public sealed class OrderResponse
    {
        [JsonPropertyName("member")]
        public OrderResponseMember[] Members { get; set; }
    }

    public sealed class OrderResponseMember
    {
        public int OrderId { get; set; }
    }
}
