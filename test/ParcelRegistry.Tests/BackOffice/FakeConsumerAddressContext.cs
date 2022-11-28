namespace ParcelRegistry.Tests.BackOffice
{
    using System;
    using Consumer.Address;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Parcel;

    public class FakeConsumerAddressContext : ConsumerAddressContext
    {
        // This needs to be here to please EF
        public FakeConsumerAddressContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeConsumerAddressContext(DbContextOptions<ConsumerAddressContext> options)
            : base(options) { }

        public void AddAddress(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus status,
            bool isRemoved = false)
        {
            AddressConsumerItems.Add(new AddressConsumerItem(
                addressPersistentLocalId,
                Guid.Empty,
                status,
                isRemoved));
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            base.OnConfiguring(optionsBuilder);
        }
    }

    public class FakeConsumerAddressContextFactory : IDesignTimeDbContextFactory<FakeConsumerAddressContext>
    {
        public FakeConsumerAddressContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<ConsumerAddressContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeConsumerAddressContext(builder.Options);
        }
    }
}
