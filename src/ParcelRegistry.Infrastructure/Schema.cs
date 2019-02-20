namespace ParcelRegistry.Infrastructure
{
    public class Schema
    {
        public const string Default = "ParcelRegistry";

        public const string Legacy = "ParcelRegistryLegacy";
        public const string Extract = "ParcelRegistryExtract";
        public const string Syndication = "ParcelRegistrySyndication";
        public const string Sequence = "AddressRegistrySequence";
    }

    public class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
        public const string Sequence = "__EFMigrationsHistorySequence";
    }
}
