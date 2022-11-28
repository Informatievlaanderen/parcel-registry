namespace ParcelRegistry.Infrastructure
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.SqlStreamStore;
    using Autofac;
    using Autofac.Core.Registration;
    using Microsoft.Extensions.Configuration;

    public static class ContainerBuilderExtensions
    {
        public static IModuleRegistrar RegisterEventStreamModule(this IModuleRegistrar builder, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Events");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'Events' connectionstring.");
            }

            builder
                .RegisterModule(new SqlStreamStoreModule(connectionString, Schema.Default))
                .RegisterModule(new TraceSqlStreamStoreModule(configuration["DataDog:ServiceName"]));

            return builder;
        }

        public static void RegisterSnapshotModule(this IModuleRegistrar builder, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Snapshots");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing 'Snapshots' connectionstring.");
            }

            builder
                .RegisterModule(new SqlSnapshotStoreModule(connectionString, Schema.Default));
        }
    }
}
