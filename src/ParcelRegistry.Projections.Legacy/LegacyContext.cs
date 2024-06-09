namespace ParcelRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using ParcelSyndication;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string ParcelDetailListCountName = "vw_ParcelDetailListCount";
        public DbSet<ParcelDetail.ParcelDetail> ParcelDetails { get; set; }

        public DbSet<ParcelSyndicationItem> ParcelSyndication { get; set; }

        public DbSet<ParcelDetailV2ListViewCount> ParcelDetailListViewCount { get; set; }

        // This needs to be here to please EF
        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParcelDetailV2ListViewCount>()
                .HasNoKey()
                .ToView(ParcelDetailListCountName, Schema.Legacy);
        }
    }

    public class ParcelDetailV2ListViewCount
    {
        public long Count { get; set; }
    }
}
