namespace ParcelRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using ParcelDetailV2;
    using ParcelSyndication;
    using ParcelDetailAddressWithCountV2 = ParcelDetailWithCountV2.ParcelDetailAddressV2;
    using ParcelDetailWithCount = ParcelDetailWithCountV2.ParcelDetailV2;


    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string ParcelDetailV2ListCountName = "vw_ParcelDetailV2ListCount";

        public DbSet<ParcelDetailV2.ParcelDetailV2> ParcelDetailV2 { get; set; }
        public DbSet<ParcelDetailWithCount> ParcelDetailWithCountV2 { get; set; }

        public DbSet<ParcelDetailAddressV2> ParcelAddressesV2 { get; set; }
        public DbSet<ParcelDetailAddressWithCountV2> ParcelAddressesWithCountV2 { get; set; }

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
}
