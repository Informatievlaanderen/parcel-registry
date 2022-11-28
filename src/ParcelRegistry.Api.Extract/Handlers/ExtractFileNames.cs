namespace ParcelRegistry.Api.Extract.Handlers
{
    using System;

    public static class ExtractFileNames
    {
        public const string ZipName = "Perceel";
        public static string FileName => $"{ZipName}-{DateTime.Now:yyyy-MM-dd}";
    }
}
