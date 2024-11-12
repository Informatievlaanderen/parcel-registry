namespace ParcelRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using ParcelLatestItem;
    using ParcelLatestItemV2;
    using ParcelRegistry.Infrastructure;
    using ParcelVersion;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<ParcelLatestItem.ParcelLatestItem> ParcelLatestItems => Set<ParcelLatestItem.ParcelLatestItem>();
        public DbSet<ParcelVersion.ParcelVersion> ParcelVersions => Set<ParcelVersion.ParcelVersion>();
        public DbSet<ParcelLatestItemAddress> ParcelLatestItemAddresses => Set<ParcelLatestItemAddress>();
        public DbSet<ParcelVersionAddress> ParcelVersionAddresses => Set<ParcelVersionAddress>();

        public DbSet<ParcelLatestItemV2.ParcelLatestItemV2> ParcelLatestItemsV2 => Set<ParcelLatestItemV2.ParcelLatestItemV2>();
        public DbSet<ParcelLatestItemV2Address> ParcelLatestItemV2Addresses => Set<ParcelLatestItemV2Address>();

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
