namespace ParcelRegistry.Producer.Ldes
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;
    using ParcelRegistry.Infrastructure;

    public class ProducerContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<ProducerContext>
    {
        public ProducerContextMigrationFactory()
            : base("ProducerLdesProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.ProducerLdes,
                Table = MigrationTables.ProducerLdes
            };

        protected override ProducerContext CreateContext(DbContextOptions<ProducerContext> migrationContextOptions)
            => new ProducerContext(migrationContextOptions);
    }
}
