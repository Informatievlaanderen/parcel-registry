namespace ParcelRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using ParcelExtract;
    using ParcelLinkExtract;

    public class ExtractContext : RunnerDbContext<ExtractContext>
    {
        public override string ProjectionStateSchema => Schema.Extract;

        public DbSet<ParcelExtractItemV2> ParcelExtractV2 { get; set; }
        public DbSet<ParcelLinkExtractItem> ParcelLinkExtract { get; set; }
        public DbSet<ParcelLinkExtractWithCount.ParcelLinkExtractItem> ParcelLinkExtractWithCount { get; set; }

        // This needs to be here to please EF
        public ExtractContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ExtractContext(DbContextOptions<ExtractContext> options)
            : base(options) { }
    }
}
