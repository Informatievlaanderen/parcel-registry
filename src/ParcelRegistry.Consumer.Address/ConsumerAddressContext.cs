namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Parcel;
    using Parcel.DataStructures;
    using ParcelRegistry.Infrastructure;

    public class ConsumerAddressContext : RunnerDbContext<ConsumerAddressContext>, IAddresses
    {
        public DbSet<AddressConsumerItem> AddressConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerAddressContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerAddressContext(DbContextOptions<ConsumerAddressContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerAddress;

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

    public class ConsumerContextFactory : RunnerDbContextMigrationFactory<ConsumerAddressContext>
    {
        public ConsumerContextFactory()
            : this("ConsumerAddressAdmin")
        { }

        public ConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.ConsumerAddress,
                Table = MigrationTables.ConsumerAddress
            })
        { }

        protected override ConsumerAddressContext CreateContext(DbContextOptions<ConsumerAddressContext> migrationContextOptions) => new ConsumerAddressContext(migrationContextOptions);

        public ConsumerAddressContext Create(DbContextOptions<ConsumerAddressContext> options) => CreateContext(options);
    }
}
