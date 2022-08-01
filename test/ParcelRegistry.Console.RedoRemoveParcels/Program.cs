namespace ParcelRegistry.Console.RedoRemoveParcels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using Api.CrabImport.CrabImport.Requests;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Parcel.Commands.Fixes;

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

            var eventsConnectionString = configuration.GetConnectionString("Events");
            _ = new JsonSerializerSettings().ConfigureDefaultForEvents();
            var commandsJsonSerializerSettings = new JsonSerializerSettings().ConfigureForCrabImports();

            var appSettings = new ApplicationSettings(configuration.GetSection("ApplicationSettings"));

            Directory.CreateDirectory(FilesToProcessPath);

            var parcelIds = GetParcelIds(eventsConnectionString);

            Console.WriteLine($"{parcelIds.Count}");

            Parallel.ForEach(parcelIds, parcelId =>
            {
                CreateCommand(new ParcelId(Guid.Parse(parcelId)), commandsJsonSerializerSettings);
            });

            ReadFilesAndSendCommands(commandsJsonSerializerSettings, appSettings);
        }

        private static void ReadFilesAndSendCommands(
            JsonSerializerSettings commandsJsonSerializerSettings,
            ApplicationSettings appSettings)
        {
            const string processedPath = "Processed";
            Directory.CreateDirectory(processedPath);

            foreach (var file in Directory.GetFiles(FilesToProcessPath))
            {
                var commandsToSend = new List<RegisterCrabImportRequest[]>();
                var command = JsonConvert.DeserializeObject<RegisterCrabImportRequest>(File.ReadAllText(file), commandsJsonSerializerSettings);

                commandsToSend.Add(new[] { command });

                SendCommands(commandsToSend, commandsJsonSerializerSettings, appSettings);

                File.Move(file, Path.Combine(processedPath, Path.GetFileName(file)));
            }
        }

        private static void CreateCommand(ParcelId parcelId, JsonSerializerSettings commandsJsonSerializerSettings)
        {
            var command = new FixGrar1475(parcelId);
            var path = Path.Combine(FilesToProcessPath, $"{parcelId:D}.json");
            var fileNr = 2;

            while (File.Exists(path))
            {
                path = Path.Combine(FilesToProcessPath, $"{parcelId:D9}-{fileNr}.json");
                fileNr++;
            }

            File.WriteAllText(
                Path.Combine(path),
                JsonConvert.SerializeObject(
                    new RegisterCrabImportRequest
                    {
                        CrabItem = JsonConvert.SerializeObject(
                            command,
                            commandsJsonSerializerSettings),
                        Type = typeof(FixGrar1475).FullName
                    }, commandsJsonSerializerSettings));
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

        private static List<string> GetParcelIds(string sqlConnectionString)
        {
            using var sqlConnection = new SqlConnection(sqlConnectionString);
            return sqlConnection.Query<string>(@"SELECT s.Id FROM [parcel-registry-events].[ParcelRegistry].[Streams] s
                    INNER JOIN [parcel-registry-events].[ParcelRegistry].[Messages] m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'ParcelWasRemoved'
                    INNER JOIN [parcel-registry-events].[ParcelRegistry].[Messages] ma on s.IdInternal = ma.StreamIdInternal and ma.[Type] = 'ParcelAddressWasAttached'
                    GROUP BY s.Id
                    HAVING max(ma.position) > max(m.position)", commandTimeout: 3600).ToList(); //not many parcels are removed, can take a while before finding
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
