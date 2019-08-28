namespace ParcelRegistry.Projections.Syndication
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;
    using Address;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;

    public class SyndicationContext : RunnerDbContext<SyndicationContext>
    {
        public override string ProjectionStateSchema => Schema.Syndication;

        // This needs to be here to please EF
        public SyndicationContext() { }

        public DbSet<AddressPersistentLocalIdItem> AddressPersistentLocalIds { get; set; }

        // This needs to be DbContextOptions<T> for Autofac!
        public SyndicationContext(DbContextOptions<SyndicationContext> options)
            : base(options) { }
    }

    public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<SyndicationContext>
    {
        public SyndicationContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "SyndicationProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<SyndicationContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Syndication, Schema.Syndication);
                })
                .UseExtendedSqlServerMigrations();

            return new SyndicationContext(builder.Options);
        }
    }
}
