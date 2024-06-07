namespace ParcelRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using ParcelSyndication;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string ParcelDetailV2ListCountName = "vw_ParcelDetailV2ListCount";
        public DbSet<ParcelDetail.ParcelDetail> ParcelDetailWithCountV2 { get; set; }

        public DbSet<ParcelSyndicationItem> ParcelSyndication { get; set; }

        public DbSet<ParcelDetailV2ListViewCount> ParcelDetailV2ListViewCount { get; set; }

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
                .ToView(ParcelDetailV2ListCountName, Schema.Legacy);
        }
    }

    public class ParcelDetailV2ListViewCount
    {
        public long Count { get; set; }
    }
}
