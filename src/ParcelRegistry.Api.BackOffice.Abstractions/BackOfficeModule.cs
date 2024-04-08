namespace ParcelRegistry.Api.BackOffice.Abstractions
{
    using Autofac;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class BackOfficeModule : Module
    {
        public BackOfficeModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var connectionString = configuration.GetConnectionString("BackOffice");

            services
                .AddDbContext<BackOfficeContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions
                        .EnableRetryOnFailure()
                        .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                    ), serviceLifetime);
        }
    }
}
