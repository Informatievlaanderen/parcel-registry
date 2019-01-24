using System.Diagnostics;

namespace ParcelRegistry.Importer
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Commandline;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Serilog;
    using Newtonsoft.Json;
    using Properties;
    using Serilog;
    using Serilog.Events;

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
                var settings = new SettingsBasedConfig();

                var generator = new CommandGenerator();

                MapLogging.Log = s => _commandCounter++;

                var builder = new CommandProcessorBuilder<CaPaKey>(generator)
                    .UseCommandLineArgs(
                        args,
                        settings.LastRunDate,
                        settings.EndDateRecovery,
                        settings.TimeMargin,
                        CaPaKey.CreateFrom,
                        errors => WaitForExit("Could not parse commandline options."));

                builder.ConfigureProcessedKeySerialization(key => key.VbrCaPaKey.ToString(), CaPaKey.CreateFrom);

                WaitForStart();

                settings.EndDateRecovery = builder.Options.Until;

                builder
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
                    .BuildAndRun();

                settings.LastRunDate = settings.EndDateRecovery;
                settings.EndDateRecovery = null;

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
