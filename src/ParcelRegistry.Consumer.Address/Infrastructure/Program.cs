namespace ParcelRegistry.Consumer.Address.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Confluent.Kafka;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using ParcelRegistry.Api.BackOffice.Abstractions;
    using Serilog;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public sealed class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var cancellationToken = CancellationTokenSource.Token;

            cancellationToken.Register(() => Closing.Set());
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

            Log.Information("Starting ParcelRegistry.Consumer.Address");

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            var loggerFactory = container.GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger<Program>();

                            async Task<Offset?> GetBackOfficeConsumerOffset(IServiceProvider serviceProvider, string topic)
                            {
                                if (long.TryParse(configuration["BackOfficeConsumerTopicOffset"], out var offset))
                                {
                                    var context = serviceProvider.GetRequiredService<ConsumerAddressContext>();
                                    if (await context.AddressConsumerItems.AnyAsync(cancellationToken))
                                    {
                                        throw new InvalidOperationException(
                                            "Cannot start consumer from offset, because consumer context already has data. Remove offset or clear data to continue.");
                                    }

                                    logger.LogInformation($"BackOfficeConsumer starting {topic} from offset {offset}.");
                                    return new Offset(offset);
                                }

                                logger.LogInformation($"BackOfficeConsumer continuing {topic} from last offset.");
                                return null;
                            }

                            await MigrationsHelper.RunAsync(configuration.GetConnectionString("ConsumerAddressAdmin"),
                                loggerFactory, cancellationToken);

                            var bootstrapServers = configuration["Kafka:BootstrapServers"];
                            var kafkaOptions = new KafkaOptions(
                                bootstrapServers,
                                configuration["Kafka:SaslUserName"],
                                configuration["Kafka:SaslPassword"],
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            var topic = $"{configuration["AddressTopic"]}" ?? throw new ArgumentException("Configuration has no AddressTopic.");

                            var lifetimeScope = container.GetRequiredService<ILifetimeScope>();
                            
                            var backOfficeConsumerOffset = await GetBackOfficeConsumerOffset(container, topic);

                            var backOfficeConsumer = new BackOfficeConsumer(
                                lifetimeScope,
                                loggerFactory,
                                kafkaOptions,
                                topic,
                                configuration["BackOfficeConsumerGroupSuffix"],
                                backOfficeConsumerOffset);
                            var backOfficeConsumerTask = backOfficeConsumer.Start(cancellationToken);

                            var consumerTasks = new List<Task> {backOfficeConsumerTask};

                            Log.Information("The kafka BackOfficeConsumer has started");

                            var enableCommandHandlingConsumer = configuration["FeatureToggles:EnableCommandHandlingConsumer"];
                            if (enableCommandHandlingConsumer != null && bool.Parse(enableCommandHandlingConsumer))
                            {
                                var commandHandlingConsumer = new CommandHandlingConsumer(
                                    lifetimeScope,
                                    loggerFactory,
                                    kafkaOptions,
                                    topic,
                                    configuration["CommandHandlingConsumerGroupSuffix"]);
                                var commandHandlingConsumerTask = commandHandlingConsumer.Start(cancellationToken);
                                consumerTasks.Add(commandHandlingConsumerTask);
                                
                                Log.Information("The kafka CommandHandlingConsumer has started");
                            }

                            await Task.WhenAny(consumerTasks);

                            CancellationTokenSource.Cancel();

                            Log.Error("The Address Consumers were terminated");
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetService<ILogger<Program>>()!);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(1000, default);
                throw;
            }

            Log.Information("Stopping...");
            Closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder
                .RegisterModule(new DataDogModule(configuration))
                .RegisterModule(new ConsumerAddressModule(configuration, services, loggerFactory, ServiceLifetime.Transient))
                .RegisterModule(new BackOfficeModule(configuration, services, loggerFactory));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
