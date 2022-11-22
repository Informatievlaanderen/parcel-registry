namespace ParcelRegistry.Console.FixParcelSnapshot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using Api.CrabImport.CrabImport;
    using Api.CrabImport.CrabImport.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
    using Legacy;
    using Legacy.Commands.Crab;
    using Legacy.Commands.Fixes;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    //Fix parcel state where invalid snapshot possible made invalid state
    //https://vlaamseoverheid.atlassian.net/wiki/spaces/VBR/pages/6144787181/Perceelregister+-+invalide+objecten+door+snapshots
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

            
            _ = new JsonSerializerSettings().ConfigureDefaultForEvents();
            var commandsJsonSerializerSettings = new JsonSerializerSettings().ConfigureForCrabImports();

            var appSettings = new ApplicationSettings(configuration.GetSection("ApplicationSettings"));

            Directory.CreateDirectory(FilesToProcessPath);

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
                var command = CreateCommand(JsonConvert.DeserializeObject<RegisterCrabImportRequest[]>(File.ReadAllText(file), commandsJsonSerializerSettings), commandsJsonSerializerSettings);
                
                commandsToSend.Add(new[] { command });

                SendCommands(commandsToSend, commandsJsonSerializerSettings, appSettings);

                File.Move(file, Path.Combine(processedPath, Path.GetFileName(file)));
            }
        }

        private static RegisterCrabImportRequest CreateCommand(RegisterCrabImportRequest[] importCommands, JsonSerializerSettings commandsJsonSerializerSettings)
        {
            JsonConvert.DefaultSettings = () => commandsJsonSerializerSettings;

            var commandsPerCommandId = importCommands
                .Select(RegisterCrabImportRequestMapping.Map)
                .Distinct(new LambdaEqualityComparer<dynamic>(x => (string)x.CreateCommandId().ToString()))
                .ToDictionary(x => (Guid?)x.CreateCommandId(), x => x);

            var aggregate = CreateAggregate(commandsPerCommandId);

            var commandToSend = new FixGrar3581(
                aggregate.ParcelId,
                aggregate.IsRealized ? ParcelStatus.Realized : ParcelStatus.Retired,
                aggregate.AddressIds);

            return new RegisterCrabImportRequest
            {
                CrabItem = JsonConvert.SerializeObject(
                    commandToSend,
                    commandsJsonSerializerSettings),
                Type = typeof(FixGrar3581).FullName
            };
        }

        private static Parcel? CreateAggregate(Dictionary<Guid?, dynamic> commandsPerCommandId)
        {
            Parcel? aggregate = null;

            foreach (var commandToProcess in commandsPerCommandId.Select(x => x.Value))
            {
                switch (commandToProcess)
                {
                    case ImportTerrainObjectFromCrab command:
                        aggregate ??= Parcel.Register(ParcelId.CreateFor(command.CaPaKey), command.CaPaKey,
                            new ParcelFactory(new NoSnapshotStrategy()));

                        aggregate.ImportTerrainObjectFromCrab(
                            command.TerrainObjectId,
                            command.IdentifierTerrainObject,
                            command.TerrainObjectNatureCode,
                            command.XCoordinate,
                            command.YCoordinate,
                            command.BuildingNature,
                            command.Lifetime,
                            command.Timestamp,
                            command.Operator,
                            command.Modification,
                            command.Organisation);
                        break;

                    case ImportTerrainObjectHouseNumberFromCrab command:
                        aggregate!.ImportTerrainObjectHouseNumberFromCrab(
                            command.TerrainObjectHouseNumberId,
                            command.TerrainObjectId,
                            command.HouseNumberId,
                            command.Lifetime,
                            command.Timestamp,
                            command.Operator,
                            command.Modification,
                            command.Organisation);
                        break;

                    case ImportSubaddressFromCrab command:
                        aggregate!.ImportSubaddressFromCrab(
                            command.SubaddressId,
                            command.HouseNumberId,
                            command.BoxNumber,
                            command.BoxNumberType,
                            command.Lifetime,
                            command.Timestamp,
                            command.Operator,
                            command.Modification,
                            command.Organisation);
                        break;
                }
            }

            return aggregate;
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
