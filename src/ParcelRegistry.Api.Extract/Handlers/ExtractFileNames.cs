namespace ParcelRegistry.Api.Extract.Handlers
{
    using System;

    public static class ExtractFileNames
    {
        public const string ParcelExtractZipName = "Perceel";
        public const string ParcelLinkExtractZipName = "AdresPerceelKoppelingen";
        public static string ParcelExtractFileName => $"{ParcelExtractZipName}-{DateTime.Now:yyyy-MM-dd}";
        public static string ParcelLinkExtractFileName => $"{ParcelLinkExtractZipName}-{DateTime.Now:yyyy-MM-dd}";
    }
}
