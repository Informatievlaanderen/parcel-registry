using System.IO;

namespace ParcelRegistry.Importer
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Serilog;
    using Newtonsoft.Json;
    using Properties;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Diagnostics;
    using System.Reflection;

    internal class Program
    {
        private static Stopwatch _stopwatch;
        private static int _commandCounter;

        private static void Main(params string[] args)
        {
            var configureForBuildingRegistry = JsonSerializerSettingsProvider.CreateSerializerSettings().ConfigureForPerceelregister();
            JsonConvert.DefaultSettings = () => configureForBuildingRegistry;

            try
            {
                var options = new ImportOptions(
                    args,
                    errors => WaitForExit("Could not parse commandline options."));
                var settings = new SettingsBasedConfig();

                MapLogging.Log = s => _commandCounter++;

                var commandProcessor = new CommandProcessorBuilder<CaPaKey>(new CommandGenerator())
                    .WithCommandLineOptions(options.ImportArguments)
                    .UseSerilog(cfg => cfg
                        .WriteTo.File(
                            "tracing.log",
                            LogEventLevel.Verbose,
                            retainedFileCountLimit: 20,
                            fileSizeLimitBytes: Settings.Default.LogFileSizeLimitBytes,
                            rollOnFileSizeLimit: true,
                            rollingInterval: RollingInterval.Day)
                        .WriteTo.Console(LogEventLevel.Information))
                    .UseHttpApiProxyConfig(settings)
                    .UseCommandProcessorConfig(settings)
                    .UseDefaultSerializerSettingsForCrabImports()
                    .ConfigureProcessedKeySerialization(a => a.VbrCaPaKey, CaPaKey.CreateFrom)
                    .ConfigureImportFeedFromAssembly(Assembly.GetExecutingAssembly())
                    .Build();

                WaitForStart();

                commandProcessor.Run(options, settings);

                WaitForExit();
            }
            catch (Exception exception)
            {
                WaitForExit("General error occurred", exception);
            }
        }

        private static void WaitForExit(
            string errorMessage = null,
            Exception exception = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
                Console.Error.WriteLine(errorMessage);

            if (exception != null)
                Console.Error.WriteLine(exception);

            Console.WriteLine();

            if (_stopwatch != null)
            {
                var avg = _commandCounter / _stopwatch.Elapsed.TotalSeconds;
                var summary = $"Report: generated {_commandCounter} commands in {_stopwatch.Elapsed}ms (={avg}/second).";
                Console.WriteLine(summary);
            }

            Console.WriteLine("Done! Press ENTER key to exit...");
            ConsoleExtensions.WaitFor(ConsoleKey.Enter);

            if (!string.IsNullOrEmpty(errorMessage))
                Environment.Exit(1);

            Environment.Exit(0);
        }

        private static void WaitForStart()
        {
            Console.WriteLine("Press ENTER key to start the CRAB Import!");
            ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            _stopwatch = Stopwatch.StartNew();
        }
    }
}
