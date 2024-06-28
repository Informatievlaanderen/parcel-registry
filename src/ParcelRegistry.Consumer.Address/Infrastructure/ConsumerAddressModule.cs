namespace ParcelRegistry.Consumer.Address.Infrastructure
{
    using System;
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
                RunOnSqlServer(services, serviceLifetime, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            services.AddScoped<IAddresses, ConsumerAddressContext>();
            return services;
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ServiceLifetime serviceLifetime,
            ILoggerFactory loggerFactory,
            string consumerProjectionsConnectionString)
        {
            services
                .AddDbContext<ConsumerAddressContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(consumerProjectionsConnectionString, sqlServerOptions =>
                    {
                        //sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                        sqlServerOptions.UseNetTopologySuite();
                        sqlServerOptions.CommandTimeout(120);
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
