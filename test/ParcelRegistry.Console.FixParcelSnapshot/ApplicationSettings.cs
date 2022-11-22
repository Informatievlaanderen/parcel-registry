namespace ParcelRegistry.Console.FixParcelSnapshot
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class ApplicationSettings
    {
        public string EndpointBaseUrl { get; set; }
        public string EndpointUrl { get; set; }
        public int HttpTimeoutInMinutes { get; set; }

        public ApplicationSettings(IConfigurationSection section)
        {
            EndpointBaseUrl = section["BaseUrl"];
            EndpointUrl = section["ImportEndpoint"];
            HttpTimeoutInMinutes = Convert.ToInt32(section["HttpTimeoutInMinutes"]);
        }
    }
}
