namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.IO;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Parcel;
    using Parcel.DataStructures;
    using ParcelRegistry.Infrastructure;

    public class ConsumerAddressContext : ConsumerDbContext<ConsumerAddressContext>, IAddresses
    {
        public override string ProcessedMessagesSchema => Schema.ConsumerAddress;

        public DbSet<AddressConsumerItem> AddressConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerAddressContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerAddressContext(DbContextOptions<ConsumerAddressContext> options)
            : base(options)
        { }

        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId)
        {
            var item = AddressConsumerItems
                .AsNoTracking()
                .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (item is null)
            {
                return null;
            }

            return new AddressData(new AddressPersistentLocalId(item.AddressPersistentLocalId), Map(item.Status), item.IsRemoved);
        }

        public ParcelRegistry.Parcel.DataStructures.AddressStatus Map(AddressStatus status)
        {
            if (status == AddressStatus.Proposed)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Proposed;
            }
            if (status == AddressStatus.Current)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Current;
            }
            if (status == AddressStatus.Rejected)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Rejected;
            }
            if (status == AddressStatus.Retired)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Retired;
            }

            throw new NotImplementedException($"Cannot parse {status} to AddressStatus");
        }
    }

    public sealed class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerAddressContext>
    {
        public ConsumerAddressContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerAddressAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ConsumerAddressContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                })
                .UseExtendedSqlServerMigrations();

            return new ConsumerAddressContext(builder.Options);
        }
    }
}
