namespace ParcelRegistry.Consumer.Address.Infrastructure
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Confluent.Kafka;
    using Destructurama;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Modules;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Infrastructure.Modules;
    using ParcelRegistry.Parcel;
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
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting ParcelRegistry.Consumer.Address");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, builder) =>
               {
                   builder
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                       .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
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
                        .AddScoped(s => new TraceDbConnection<BackOfficeContext>(
                            new SqlConnection(hostContext.Configuration.GetConnectionString("BackOffice")),
                            hostContext.Configuration["DataDog:ServiceName"]))
                        .AddDbContextFactory<BackOfficeContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(provider.GetRequiredService<TraceDbConnection<BackOfficeContext>>(), sqlServerOptions => sqlServerOptions
                                .EnableRetryOnFailure()
                                .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                            ));

                    services
                        .AddScoped(s => new TraceDbConnection<ConsumerAddressContext>(
                            new SqlConnection(hostContext.Configuration.GetConnectionString("ConsumerAddress")),
                            hostContext.Configuration["DataDog:ServiceName"]))
                        .AddDbContextFactory<ConsumerAddressContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(provider.GetRequiredService<TraceDbConnection<ConsumerAddressContext>>(), sqlServerOptions =>
                            {
                                sqlServerOptions.EnableRetryOnFailure();
                                sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                            }));

                    services.AddScoped<IAddresses, ConsumerAddressContext>();
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder.Register(_ =>
                    {
                        var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                        var topic = $"{hostContext.Configuration["AddressTopic"]}" ?? throw new ArgumentException("Configuration has no AddressTopic.");
                        var suffix = hostContext.Configuration["GroupSuffix"];
                        var consumerGroupId = $"{nameof(ParcelRegistry)}.{nameof(BackOfficeConsumer)}.{topic}{suffix}";

                        Offset? offset = null;
                        var offsetString = hostContext.Configuration["TopicOffset"];
                        if (!string.IsNullOrEmpty(offsetString))
                        {
                            if (!long.TryParse(offsetString, out var offsetAsLong))
                            {
                                throw new ArgumentException("Configuration TopicOffset is not a valid value.");
                            }

                            offset = new Offset(offsetAsLong);
                        }
                        
                        return new IdempotentKafkaConsumerOptions(
                            bootstrapServers,
                            hostContext.Configuration["Kafka:SaslUserName"],
                            hostContext.Configuration["Kafka:SaslPassword"],
                            consumerGroupId,
                            topic,
                            noMessageFoundDelay: 300,
                            offset,
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
                    });

                    builder
                        .RegisterType<KafkaIdompotencyConsumer<ConsumerAddressContext>>()
                        .As<IKafkaIdompotencyConsumer<ConsumerAddressContext>>()
                        .SingleInstance();

                    builder
                        .RegisterModule(new DataDogModule(hostContext.Configuration))
                        .RegisterModule(new EditModule(hostContext.Configuration))
                        .RegisterModule(new BackOfficeModule(hostContext.Configuration, services, loggerFactory, ServiceLifetime.Transient));

                    builder
                        .RegisterType<BackOfficeConsumer>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting ParcelRegistry.Consumer.Address");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        await MigrationsHelper.RunAsync(
                            configuration.GetConnectionString("ConsumerAddressAdmin"),
                            loggerFactory,
                            CancellationToken.None);

                        await host.RunAsync().ConfigureAwait(false);
                    },
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
