namespace ParcelRegistry.Api.CrabImport.Infrastructure
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        private static readonly Tuple<string, string> DevelopmentCertificate = new Tuple<string, string>(
            "api.dev.perceel.basisregisters.vlaanderen.be.pfx",
            "gemeenteregister!");

        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => new WebHostBuilder()
                .UseDefaultForApi<Startup>(
                    httpPort: 1095,
                    httpsPort: 1449,
                    httpsCertificate: () => new X509Certificate2(DevelopmentCertificate.Item1, DevelopmentCertificate.Item2),
                    commandLineArgs: args);
    }
}
