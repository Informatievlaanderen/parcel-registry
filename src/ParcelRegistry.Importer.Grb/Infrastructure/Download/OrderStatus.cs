namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    using System.Text.Json.Serialization;

    public enum OrderStatus
    {
        Unknown = 0,
        [JsonPropertyName("Received")]
        Received = 1,
        [JsonPropertyName("Processing")]
        Processing = 2,
        [JsonPropertyName("Completed")]
        Completed = 3
    }
}
