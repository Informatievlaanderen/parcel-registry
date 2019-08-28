namespace ParcelRegistry.Projections.Extract
{
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using ParcelExtract;

    public class ExtractContext : RunnerDbContext<ExtractContext>
    {
        public override string ProjectionStateSchema => Schema.Extract;

        public DbSet<ParcelExtractItem> ParcelExtract { get; set; }

        // This needs to be here to please EF
        public ExtractContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ExtractContext(DbContextOptions<ExtractContext> options)
            : base(options) { }
    }

    public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<ExtractContext>
    {
        public ExtractContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ExtractProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            var builder = new DbContextOptionsBuilder<ExtractContext>()
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Extract, Schema.Extract);
                })
                .UseExtendedSqlServerMigrations();

            return new ExtractContext(builder.Options);
        }
    }
}
