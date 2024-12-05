namespace ParcelRegistry.Consumer.Address.Console
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.Extensions;
    using Destructurama;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Parcel;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Infrastructure.Modules;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;
    using MigrationsHelper = Infrastructure.MigrationsHelper;

    public sealed class Program
    {
        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting ParcelRegistry.Consumer.Address");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, builder) =>
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
                        .AddDbContextFactory<BackOfficeContext>((_, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("BackOffice"), sqlServerOptions => sqlServerOptions
                                .EnableRetryOnFailure()
                                .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                            ));

                    services
                        .AddDbContextFactory<ConsumerAddressContext>((_, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("ConsumerAddress"), sqlServerOptions =>
                            {
                                sqlServerOptions.EnableRetryOnFailure();
                                sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                                sqlServerOptions.UseNetTopologySuite();
                            }));

                    services.AddScoped<IAddresses, ConsumerAddressContext>();
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder.Register(c =>
                    {
                        var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                        var topic = $"{hostContext.Configuration["AddressTopic"]}" ?? throw new ArgumentException("Configuration has no AddressTopic.");
                        var suffix = hostContext.Configuration["GroupSuffix"];
                        var consumerGroupId = $"ParcelRegistry.BackOfficeConsumer.{topic}{suffix}";

                        var consumerOptions = new ConsumerOptions(
                            new BootstrapServers(bootstrapServers),
                            new Topic(topic),
                            new ConsumerGroupId(consumerGroupId),
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                        consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                            hostContext.Configuration["Kafka:SaslUserName"],
                            hostContext.Configuration["Kafka:SaslPassword"]));

                        using var ctx = c.Resolve<ConsumerAddressContext>();
                        ctx.OverrideConfigureOffset(consumerOptions);

                        return consumerOptions;
                    });

                    builder
                        .RegisterType<IdempotentConsumer<ConsumerAddressContext>>()
                        .As<IIdempotentConsumer<ConsumerAddressContext>>()
                        .SingleInstance();

                    services.ConfigureIdempotency(
                        hostContext.Configuration.GetSection(IdempotencyConfiguration.Section)
                            .Get<IdempotencyConfiguration>()!.ConnectionString!,
                        new IdempotencyMigrationsTableInfo(Schema.Import),
                        new IdempotencyTableInfo(Schema.Import),
                        loggerFactory);

                    builder.RegisterType<IdempotentCommandHandler>()
                        .As<IIdempotentCommandHandler>()
                        .AsSelf()
                        .InstancePerLifetimeScope();

                    builder
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
