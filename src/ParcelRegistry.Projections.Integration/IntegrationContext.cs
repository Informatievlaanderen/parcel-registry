﻿namespace ParcelRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using ParcelLatestItem;
    using ParcelRegistry.Infrastructure;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<ParcelLatestItem.ParcelLatestItem> ParcelLatestItems => Set<ParcelLatestItem.ParcelLatestItem>();
        public DbSet<ParcelLatestItem.ParcelLatestItemAddress> ParcelLatestItemAddresses => Set<ParcelLatestItemAddress>();

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
