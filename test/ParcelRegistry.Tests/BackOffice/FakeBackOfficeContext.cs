namespace ParcelRegistry.Tests.BackOffice
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions;

    public class FakeBackOfficeContext : BackOfficeContext
    {
        // This needs to be here to please EF
        public FakeBackOfficeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeBackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public void AddParcelAddressRelation(
            ParcelId parcelId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            ParcelAddressRelations.Add(new ParcelAddressRelation(parcelId, addressPersistentLocalId));
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            base.OnConfiguring(optionsBuilder);
        }
    }

    public class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<FakeBackOfficeContext>
    {
        public FakeBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeBackOfficeContext(builder.Options);
        }
    }
}
