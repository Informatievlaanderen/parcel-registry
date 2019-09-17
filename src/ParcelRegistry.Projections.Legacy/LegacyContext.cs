namespace ParcelRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using ParcelDetail;
    using ParcelSyndication;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<ParcelDetail.ParcelDetail> ParcelDetail { get; set; }
        public DbSet<ParcelDetailAddress> ParcelAddresses { get; set; }
        public DbSet<ParcelSyndicationItem> ParcelSyndication { get; set; }

        // This needs to be here to please EF
        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }
}
