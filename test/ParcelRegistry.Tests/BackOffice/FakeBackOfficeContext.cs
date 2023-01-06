namespace ParcelRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions;

    public class FakeBackOfficeContext : BackOfficeContext
    {
        private readonly bool _dispose;

        // This needs to be here to please EF
        public FakeBackOfficeContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeBackOfficeContext(DbContextOptions<BackOfficeContext> options, bool dispose = true)
            : base(options)
        {
            _dispose = dispose;
        }

        public void AddParcelAddressRelation(ParcelId parcelId, AddressPersistentLocalId addressPersistentLocalId)
        {
            ParcelAddressRelations.Add(new ParcelAddressRelation(parcelId, addressPersistentLocalId));
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            base.OnConfiguring(optionsBuilder);
        }

        public override ValueTask DisposeAsync()
        {
            if (_dispose)
            {
                return base.DisposeAsync();
            }

            return new ValueTask(Task.CompletedTask);
        }
    }

    public class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<FakeBackOfficeContext>
    {
        private readonly bool _dispose;

        public FakeBackOfficeContextFactory(bool dispose = true)
        {
            _dispose = dispose;
        }

        public FakeBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeBackOfficeContext(builder.Options, _dispose);
        }
    }
}
