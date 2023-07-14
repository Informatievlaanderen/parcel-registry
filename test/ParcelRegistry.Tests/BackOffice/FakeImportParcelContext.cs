namespace ParcelRegistry.Tests.BackOffice
{
    using System;
    using System.Threading.Tasks;
    using Importer.Grb;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

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

    public class FakeImportParcelContextFactory : IDbContextFactory<ImporterContext>
    {
        private readonly bool _dispose;

        private readonly FakeImportParcelContext _context;

        public FakeImportParcelContextFactory(bool dispose = true)
        {
            _dispose = dispose;
            var builder = new DbContextOptionsBuilder<ImporterContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            _context = new FakeImportParcelContext(builder.Options, _dispose);
        }

        public ImporterContext CreateDbContext() => _context;
    }
}
