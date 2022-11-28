namespace ParcelRegistry.Api.Extract.Extracts.Responses
{
    using System;
    using ParcelRegistry.Api.Extract.Handlers;
    using Swashbuckle.AspNetCore.Filters;

    public class ParcelRegistryResponseExample : IExamplesProvider<object>
    {
        public object GetExamples()
            => new { mimeType = "application/zip", fileName = $"{ExtractFileNames.ZipName}-{DateTime.Now:yyyy-MM-dd}.zip" };
    }
}
