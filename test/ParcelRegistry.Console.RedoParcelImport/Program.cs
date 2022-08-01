namespace ParcelRegistry.Console.RedoParcelImport
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using Api.CrabImport.CrabImport.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    //Case 5f: when housenumber relation is retired then becomes active again (correction), buildings with more than 2 subaddresses don't have the right amount of units
    public class Program
    {
        const string FilesToProcessPath = "FilesToProcess";

        protected Program()
        { }
        
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var commandsJsonSerializerSettings = new JsonSerializerSettings().ConfigureForCrabImports();

            var appSettings = new ApplicationSettings(configuration.GetSection("ApplicationSettings"));

            Directory.CreateDirectory(FilesToProcessPath);

            Console.WriteLine("Sending commands");

            ReadFilesAndSendCommands(commandsJsonSerializerSettings, appSettings);

            Console.WriteLine("Commands sent");
        }

        private static void ReadFilesAndSendCommands(
            JsonSerializerSettings commandsJsonSerializerSettings,
            ApplicationSettings appSettings)
        {
            var processedPath = "Processed";
            Directory.CreateDirectory(processedPath);

            foreach (var file in Directory.GetFiles(FilesToProcessPath))
            {
                var commandsToSend = new List<RegisterCrabImportRequest[]>();
                var command = JsonConvert.DeserializeObject<RegisterCrabImportRequest[]>(File.ReadAllText(file), commandsJsonSerializerSettings);

                commandsToSend.Add(command);

                SendCommands(commandsToSend, commandsJsonSerializerSettings, appSettings);

                File.Move(file, Path.Combine(processedPath, Path.GetFileName(file)));
            }
        }

        private static void SendCommands(List<RegisterCrabImportRequest[]> commandsToSend, JsonSerializerSettings commandsJsonSerializerSettings, ApplicationSettings appSettings)
        {
            var jsonToSend = JsonConvert.SerializeObject(commandsToSend, commandsJsonSerializerSettings);
            using var client = CreateImportClient(appSettings);
            var response = client
                .PostAsync(
                    appSettings.EndpointUrl,
                    CreateJsonContent(jsonToSend))
                .GetAwaiter()
                .GetResult();

            response.EnsureSuccessStatusCode();
        }

        protected static HttpClient CreateImportClient(ApplicationSettings settings)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(settings.EndpointBaseUrl),
                Timeout = TimeSpan.FromMinutes(settings.HttpTimeoutInMinutes)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            return client;
        }

        protected static StringContent CreateJsonContent(string jsonValue)
            => new StringContent(jsonValue, Encoding.UTF8, MediaTypeNames.Application.Json);
    }
}
