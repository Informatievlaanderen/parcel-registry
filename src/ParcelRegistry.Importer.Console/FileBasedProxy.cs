namespace ParcelRegistry.Importer.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
    using Newtonsoft.Json;
    using Microsoft.Extensions.Logging;

    public class FileBasedProxyFactory : IApiProxyFactory
    {
        public static IApiProxyFactory BuildFileBasedProxyFactory(ILogger _) => new FileBasedProxyFactory();

        public IApiProxy Create() => new FileBasedProxy();
    }

    public class FileBasedProxy : IApiProxy
    {
        //29/03/2020 1:20:03 - 24/04/2020 22:05:48
        private static DateTime FromInit = new DateTime(2020, 03, 29, 01, 20, 03);
        private static DateTime UntilInit = new DateTime(2020, 09, 03, 10, 15, 03);
        private static readonly string ImportFolder = $"{FromInit:yyyy-MM-dd}-{UntilInit:yyyy-MM-dd}";
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings().ConfigureForCrabImports();
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault(SerializerSettings);

        public void ImportBatch<TKey>(IEnumerable<KeyImport<TKey>> imports)
        {
            foreach (var import in imports)
            {
                var key = import.Key as CaPaKey;
                if (import.Commands.Length != 0)
                    File.WriteAllText(
                        Path.Combine(ImportFolder, $"{key.VbrCaPaKey}.json"),
                        Serializer.Serialize(import.Commands));
            }
        }

        public ICommandProcessorOptions<TKey> GetImportOptions<TKey>(
            ImportOptions options,
            ICommandProcessorBatchConfiguration<TKey> configuration)
        {
            var batchStatus = new BatchStatus
            {
                From = FromInit,
                Until = UntilInit,
                Completed = false
            };

            if (!Directory.Exists(ImportFolder))
                Directory.CreateDirectory(ImportFolder);

            return options.CreateProcessorOptions(batchStatus, configuration);
        }

        public void InitializeImport<TKey>(ICommandProcessorOptions<TKey> options) { }

        public void FinalizeImport<TKey>(ICommandProcessorOptions<TKey> options) { }
    }
}
