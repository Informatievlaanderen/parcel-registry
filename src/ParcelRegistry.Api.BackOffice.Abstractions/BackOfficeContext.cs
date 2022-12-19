namespace ParcelRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Parcel;

    public class BackOfficeContext : DbContext
    {
        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public DbSet<ParcelAddressRelation> ParcelAddressRelations { get; set; }

        public async Task<ParcelAddressRelation> AddIdempotentParcelAddressRelation(ParcelId parcelId, AddressPersistentLocalId addressPersistentLocalId, CancellationToken cancellationToken)
        {
            var relation = await ParcelAddressRelations.FindAsync(new object?[] { (Guid)parcelId, (int) addressPersistentLocalId }, cancellationToken: cancellationToken);
            if (relation is null)
            {
                relation = new ParcelAddressRelation(parcelId, addressPersistentLocalId);
                await ParcelAddressRelations.AddAsync(relation, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }

            return relation;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParcelAddressRelation>()
                .ToTable("ParcelAddressRelation", Schema.BackOffice)
                .HasKey(x => new { x.ParcelId, x.AddressPersistentLocalId })
                .IsClustered();

            modelBuilder.Entity<ParcelAddressRelation>()
                .HasIndex(x => x.ParcelId);
            modelBuilder.Entity<ParcelAddressRelation>()
                .Property(x => x.ParcelId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ParcelAddressRelation>()
                .HasIndex(x => x.AddressPersistentLocalId);
            modelBuilder.Entity<ParcelAddressRelation>()
                .Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();
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
