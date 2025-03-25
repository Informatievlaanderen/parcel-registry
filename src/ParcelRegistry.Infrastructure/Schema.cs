namespace ParcelRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "ParcelRegistry";

        public const string Import = "ParcelRegistryImport";
        public const string Legacy = "ParcelRegistryLegacy";
        public const string Extract = "ParcelRegistryExtract";
        public const string ConsumerAddress = "ParcelRegistryConsumerAddress";
        public const string BackOffice = "ParcelRegistryBackOffice";
        public const string BackOfficeProjections = "ParcelRegistryBackOfficeProjections";
        public const string MigrateParcel = "ParcelRegistryMigration";
        public const string Producer = "ParcelRegistryProducer";
        public const string ProducerSnapshotOslo = "ParcelRegistryProducerSnapshotOslo";
        public const string ProducerLdes = "ParcelRegistryProducerLdes";
        public const string GrbImporter = "GrbImporter";
        public const string Integration = "integration_parcel";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string ConsumerAddress = "__EFMigrationsHistoryConsumerAddress";
        public const string BackOffice = "__EFMigrationsHistoryBackOffice";
        public const string BackOfficeProjections = "__EFMigrationsHistoryBackOfficeProjections";
        public const string Producer = "__EFMigrationsHistoryProducer";
        public const string ProducerSnapshotOslo = "__EFMigrationsHistoryProducerSnapshotOslo";
        public const string ProducerLdes = "__EFMigrationsHistoryProducerLdes";
        public const string GrbImporter = "__EFMigrationsHistoryGrbImporter";
        public const string Integration = "__EFMigrationsHistory";
    }
}
