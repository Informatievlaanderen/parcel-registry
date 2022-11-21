namespace ParcelRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.IO;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class BackOfficeContext : DbContext
    {
        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public DbSet<ParcelAddressRelation> ParcelAddressRelations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParcelAddressRelation>()
                .ToTable("ParcelAddressRelation", Schema.BackOffice)
                .HasKey(x => x.AddressPersistentLocalId)
                .IsClustered();

            modelBuilder.Entity<ParcelAddressRelation>()
                .Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ParcelAddressRelation>()
                .Property(x => x.ParcelId);
        }
    }

    public class ParcelAddressRelation
    {
        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }

        private ParcelAddressRelation()
        { }

        public ParcelAddressRelation(Guid parcelId, int addressPersistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
        }
    }

    public class ConfigBasedSequenceContextFactory : IDesignTimeDbContextFactory<BackOfficeContext>
    {
        public BackOfficeContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "BackOffice";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<BackOfficeContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice);
                });

            return new BackOfficeContext(builder.Options);
        }
    }
}
