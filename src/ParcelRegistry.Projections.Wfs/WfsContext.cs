namespace ParcelRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using ParcelWfs;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;

        public DbSet<ParcelWfsItem> ParcelWfsItems => Set<ParcelWfsItem>();
        public DbSet<ParcelWfsAddressItem> ParcelWfsAddresses => Set<ParcelWfsAddressItem>();

        // This needs to be here to please EF
        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options) { }
    }
}
