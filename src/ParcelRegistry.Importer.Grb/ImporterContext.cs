namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using ParcelRegistry.Infrastructure;

    public class ImporterContext : DbContext
    {
        public DbSet<RunHistory> RunHistory { get; set; }
        public DbSet<ProcessedRequests> ProcessedRequests { get; set; }

        public async Task<bool> ProcessedRequestExists(string getSha256)
        {
            return await ProcessedRequests.FindAsync(getSha256) is not null;
        }

        public async Task AddProcessedRequest(string sha256)
        {
            var processedRequest = new ProcessedRequests(sha256);
            ProcessedRequests.Add(processedRequest);
            await SaveChangesAsync();
        }

        public virtual async Task ClearProcessedRequests()
        {
            await Database.ExecuteSqlInterpolatedAsync($"TRUNCATE TABLE [{Schema.GrbImporter}].[{ProcessedRequestsConfiguration.TableName}]");
            await SaveChangesAsync();
        }

        public async Task<RunHistory> AddRunHistory(DateTimeOffset from, DateTimeOffset to)
        {
            var runHistory = new RunHistory(from, to);
            await RunHistory.AddAsync(runHistory);
            await SaveChangesAsync();

            return runHistory;
        }

        public async Task<RunHistory> GetLatestRunHistory()
        {
            return await RunHistory
                .OrderByDescending(x => x.Id)
                .FirstAsync();
        }


        public async Task CompleteRunHistory(int id)
        {
            var runHistory = await RunHistory.FindAsync(id);
            runHistory.Completed = true;
            await SaveChangesAsync();
        }

        public ImporterContext()
        { }

        public ImporterContext(DbContextOptions<ImporterContext> options)
            : base(options)
        { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ImporterContext).Assembly);
        }
    }

    public sealed class ConfigBasedImporterGrbContextFactory : IDesignTimeDbContextFactory<ImporterContext>
    {
        public ImporterContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "ImporterGrb";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ImporterContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.GrbImporter, Schema.GrbImporter);
                });

            return new ImporterContext(builder.Options);
        }
    }
}
