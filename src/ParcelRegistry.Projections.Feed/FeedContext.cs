namespace ParcelRegistry.Projections.Feed
{
    using ParcelFeed;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class FeedContext : RunnerDbContext<FeedContext>
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public override string ProjectionStateSchema => Schema.Feed;

        public DbSet<ParcelFeedItem> ParcelFeed => Set<ParcelFeedItem>();

        public DbSet<ParcelDocument> ParcelDocuments => Set<ParcelDocument>();

        // This needs to be here to please EF
        public FeedContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FeedContext(DbContextOptions<FeedContext> options, JsonSerializerSettings jsonSerializerSettings)
            : base(options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("ParcelFeedSequence", Schema.Feed)
                .StartsAt(1)
                .IncrementsBy(1)
                .IsCyclic(false);

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ParcelFeedConfiguration());
            modelBuilder.ApplyConfiguration(new ParcelDocumentConfiguration(_jsonSerializerSettings));
        }
    }
}
