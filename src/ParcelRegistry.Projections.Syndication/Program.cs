namespace ParcelRegistry.Projections.Syndication
{
    using Address;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Modules;
    using Serilog;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting BuildingRegistry.Projections.Syndication");

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await MigrationsHelper.RunAsync(
                                configuration.GetConnectionString("SyndicationProjectionsAdmin"),
                                container.GetRequiredService<ILoggerFactory>(),
                                ct);

                            await container
                                .GetRequiredService<FeedProjector<SyndicationContext>>()
                                .Register(BuildProjectionRunner(configuration, container))
                                .Start(ct);
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetRequiredService<ILogger<Program>>());
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            Log.Information("Stopping...");
            Closing.Close();
        }

        private static IFeedProjectionRunner<SyndicationContext> BuildProjectionRunner(IConfiguration configuration, IServiceProvider container)
        {
            return new FeedProjectionRunner<AddressEvent, SyndicationContent<Address.Address>, SyndicationContext>(
                "address",
                configuration.GetValue<Uri>("SyndicationFeeds:Address"),
                configuration.GetValue<string>("SyndicationFeeds:AddressAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:AddressAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:AddressPollingInMilliseconds"),
                false,
                true,
                container.GetRequiredService<ILogger<Program>>(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new AddressPersistentLocalIdProjection());
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(new SyndicationModule(configuration, services, tempProvider.GetRequiredService<ILoggerFactory>()));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
