namespace ParcelRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "ParcelRegistry";

        public const string Import = "ParcelRegistryImport";
        public const string Legacy = "ParcelRegistryLegacy";
        public const string Extract = "ParcelRegistryExtract";
        public const string Syndication = "ParcelRegistrySyndication";
        public const string ConsumerAddress = "ParcelRegistryConsumerAddress";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string Syndication = "__EFMigrationsHistorySyndication";
        public const string ConsumerAddress = "__EFMigrationsHistoryConsumerAddress";
    }
}
