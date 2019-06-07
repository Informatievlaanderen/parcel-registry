namespace ParcelRegistry.Infrastructure
{
    public class Schema
    {
        public const string Default = "ParcelRegistry";
        public const string Import = "ParcelRegistryImport";

        public const string Legacy = "ParcelRegistryLegacy";
        public const string Extract = "ParcelRegistryExtract";
        public const string Syndication = "ParcelRegistrySyndication";
    }

    public class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
    }
}
