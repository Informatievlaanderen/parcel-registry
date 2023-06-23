namespace ParcelRegistry.Consumer.Address.Infrastructure.Modules
{
    using System;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Parcel;
    using ParcelRegistry.Infrastructure;

    public static class ConsumerAddressModule
    {
        public static IServiceCollection ConfigureConsumerAddress(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger("ConsumerAddress");
            var connectionString = configuration.GetConnectionString("ConsumerAddress");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(configuration, services, serviceLifetime, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            services.AddScoped<IAddresses, ConsumerAddressContext>();
            return services;
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ServiceLifetime serviceLifetime,
            ILoggerFactory loggerFactory,
            string consumerProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<ConsumerAddressContext>(
                    new SqlConnection(consumerProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<ConsumerAddressContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<ConsumerAddressContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                        sqlServerOptions.UseNetTopologySuite();
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ConsumerAddressContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ConsumerAddressContext));
        }
    }
}
