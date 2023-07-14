namespace ParcelRegistry.Importer.Grb.Infrastructure
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Destructurama;
    using Download;
    using Handlers;
    using MediatR;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Infrastructure.Modules;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;

    public sealed class Program
    {
        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception) eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var projectName = Assembly.GetEntryAssembly().GetName().Name;
            Log.Information($"Starting {projectName}");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    SelfLog.Enable(Console.WriteLine);

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes()
                        .CreateLogger();

                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    services
                        .AddScoped(s => new TraceDbConnection<ImporterContext>(
                            new SqlConnection(hostContext.Configuration.GetConnectionString("ImporterGrb")),
                            hostContext.Configuration["DataDog:ServiceName"]))
                        .AddDbContextFactory<ImporterContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(provider.GetRequiredService<TraceDbConnection<ImporterContext>>(), sqlServerOptions => sqlServerOptions
                                .EnableRetryOnFailure()
                                .MigrationsHistoryTable(MigrationTables.GrbImporter, Schema.GrbImporter)
                            ));

                    services.Configure<DownloadClientOptions>(hostContext.Configuration.GetSection("DownloadClientOptions"));

                    services.AddHttpClient(nameof(DownloadClient), client =>
                    {
                        var url = hostContext.Configuration["DownloadUrl"] ?? throw new ArgumentNullException("DownloadUrl");
                        client.BaseAddress = new Uri(url);
                    });
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    services.RegisterModule(new DataDogModule(hostContext.Configuration));

                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder
                        .RegisterModule(new CommandHandlingModule(hostContext.Configuration))
                        .RegisterModule(new BackOfficeModule(hostContext.Configuration, services, loggerFactory));

                    builder
                        .RegisterType<Mediator>()
                        .As<IMediator>()
                        .InstancePerLifetimeScope();

                    builder
                        .RegisterAssemblyTypes(typeof(ImportParcelHandler)
                        .GetTypeInfo().Assembly)
                        .AsImplementedInterfaces();

                    builder.Register(c =>
                        new DownloadClient(
                            c.Resolve<IOptions<DownloadClientOptions>>(),
                            c.Resolve<IHttpClientFactory>(),
                            new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() },
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                PropertyNameCaseInsensitive = true
                            }));

                    builder
                        .RegisterType<DownloadFacade>()
                        .As<IDownloadFacade>();

                    builder
                        .RegisterType<ZipArchiveProcessor>()
                        .As<IZipArchiveProcessor>();

                    builder
                        .RegisterType<Importer>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                await host.Services.GetRequiredService<ImporterContext>().Database.MigrateAsync();

                await DistributedLock<Program>.RunAsync(
                        async () => { await host.RunAsync().ConfigureAwait(false); },
                        DistributedLockOptions.LoadFromConfiguration(configuration),
                        logger)
                    .ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(500, default);
                throw;
            }
            finally
            {
                logger.LogInformation("Stopping...");
            }
        }
    }
}
