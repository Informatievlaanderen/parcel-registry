namespace ParcelRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Importer.Grb;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions;

    public class FakeImportParcelContext : ImporterContext
    {
        private readonly bool _dispose;

        // This needs to be here to please EF
        public FakeImportParcelContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakeImportParcelContext(DbContextOptions<ImporterContext> options, bool dispose = true)
            : base(options)
        {
            _dispose = dispose;
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

    public class FakeImportParcelContextFactory : IDesignTimeDbContextFactory<FakeImportParcelContext>
    {
        private readonly bool _dispose;

        public FakeImportParcelContextFactory(bool dispose = true)
        {
            _dispose = dispose;
        }

        public FakeImportParcelContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<ImporterContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new FakeImportParcelContext(builder.Options, _dispose);
        }
    }
}
