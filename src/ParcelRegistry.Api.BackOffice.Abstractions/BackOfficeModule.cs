namespace ParcelRegistry.Api.BackOffice.Abstractions
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
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
                .AddScoped(s => new TraceDbConnection<BackOfficeContext>(
                    new SqlConnection(connectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<BackOfficeContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<BackOfficeContext>>(), sqlServerOptions => sqlServerOptions
                        .EnableRetryOnFailure()
                        .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                    ), serviceLifetime);
        }
    }
}
