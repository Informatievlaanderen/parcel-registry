namespace ParcelRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<WfsContext>
    {
        public WfsContextMigrationFactory()
            : base("WfsProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Wfs,
                Table = MigrationTables.Wfs
            };

        protected override WfsContext CreateContext(DbContextOptions<WfsContext> migrationContextOptions)
            => new WfsContext(migrationContextOptions);
    }
}
