namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using NetTopologySuite.Geometries;
    using Parcel;
    using Parcel.DataStructures;
    using ParcelRegistry.Infrastructure;

    public class ConsumerAddressContext : SqlServerConsumerDbContext<ConsumerAddressContext>, IAddresses, IOffsetOverrideDbSet
    {
        public override string ProcessedMessagesSchema => Schema.ConsumerAddress;

        public DbSet<AddressConsumerItem> AddressConsumerItems => Set<AddressConsumerItem>();
        public DbSet<OffsetOverride> OffsetOverrides => Set<OffsetOverride>();

        // This needs to be here to please EF
        public ConsumerAddressContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerAddressContext(DbContextOptions<ConsumerAddressContext> options)
            : base(options)
        { }

        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId)
        {
            var item = AddressConsumerItems
                .AsNoTracking()
                .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (item is null)
            {
                return null;
            }

            return new AddressData(new AddressPersistentLocalId(item.AddressPersistentLocalId), Map(item.Status), item.IsRemoved);
        }

        /// <summary>
        /// Set once the Lambert 2008 column has been observed complete. It only ever goes from false to
        /// true: the address conversion fills the column and nothing empties it again. The context is
        /// registered as a singleton in the GRB importer, so this costs one query per run rather than one
        /// per parcel.
        /// </summary>
        private bool _lambert2008PositionsVerified;

        public IEnumerable<AddressConsumerItem> FindAddressesWithinGeometry(Geometry geometry)
        {
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            // Dispatch on where the coordinates actually are, not on the SRID. GrbXmlReader reads GRB GML
            // through a GMLReader built on the Lambert 72 geometry factory, so every polygon arriving here
            // carries SRID 31370 by construction — including, once GRB delivers Lambert 2008, ones whose
            // coordinates are nothing of the sort. See ADR 0004.
            return fixedGeometry.IsInsideFlandersUsingLambert08()
                ? FindWithinLambert2008(fixedGeometry)
                : FindWithinLambert72(fixedGeometry);
        }

        private IEnumerable<AddressConsumerItem> FindWithinLambert72(Geometry fixedGeometry)
        {
            var containsResult = AddressConsumerItems
                .Where(x => !x.IsRemoved && fixedGeometry.Contains(x.Position))
                .ToList();

            var touchesResult = AddressConsumerItems
                .Where(x => !x.IsRemoved && x.Position.Touches(fixedGeometry))
                .ToList();

            return Combine(containsResult, touchesResult);
        }

        private IEnumerable<AddressConsumerItem> FindWithinLambert2008(Geometry fixedGeometry)
        {
            GuardLambert2008PositionsAreComplete();

            var containsResult = AddressConsumerItems
                .Where(x => !x.IsRemoved && fixedGeometry.Contains(x.PositionLambert2008))
                .ToList();

            var touchesResult = AddressConsumerItems
                .Where(x => !x.IsRemoved && x.PositionLambert2008!.Touches(fixedGeometry))
                .ToList();

            return Combine(containsResult, touchesResult);
        }

        /// <summary>
        /// A Lambert 2008 query while any address is still missing its Lambert 2008 position would silently
        /// skip that address, and the parcel would import without it. The conversion order — every address
        /// converted before any parcel is — makes this unreachable; this turns that assumption about
        /// another register's conversion into a stopped importer rather than parcels quietly missing
        /// addresses. See ADR 0004.
        /// </summary>
        private void GuardLambert2008PositionsAreComplete()
        {
            if (_lambert2008PositionsVerified)
            {
                return;
            }

            if (AddressConsumerItems.Any(x => !x.IsRemoved && x.PositionLambert2008 == null))
            {
                throw new InvalidOperationException(
                    "Cannot resolve addresses for a Lambert 2008 parcel: some addresses have no Lambert 2008 "
                    + "position yet, so they would be silently skipped. The address register's conversion to "
                    + "Lambert 2008 has to complete before parcels are converted.");
            }

            _lambert2008PositionsVerified = true;
        }

        private static IEnumerable<AddressConsumerItem> Combine(
            IEnumerable<AddressConsumerItem> containsResult,
            IEnumerable<AddressConsumerItem> touchesResult)
            => containsResult
                .Union(touchesResult)
                .Where(x => new[] { AddressStatus.Proposed, AddressStatus.Current }.Contains(x.Status))
                .Distinct();

        private static ParcelRegistry.Parcel.DataStructures.AddressStatus Map(AddressStatus status)
        {
            if (status == AddressStatus.Proposed)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Proposed;
            }
            if (status == AddressStatus.Current)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Current;
            }
            if (status == AddressStatus.Rejected)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Rejected;
            }
            if (status == AddressStatus.Retired)
            {
                return ParcelRegistry.Parcel.DataStructures.AddressStatus.Retired;
            }

            throw new NotImplementedException($"Cannot parse {status} to AddressStatus");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(ConsumerAddressContext).GetTypeInfo().Assembly);
            modelBuilder.ApplyConfiguration(new OffsetOverrideConfiguration(ProcessedMessagesSchema));
        }
    }

    public sealed class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerAddressContext>
    {
        public ConsumerAddressContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerAddressAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ConsumerAddressContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                    sqlServerOptions.UseNetTopologySuite();
                })
                .UseExtendedSqlServerMigrations();

            return new ConsumerAddressContext(builder.Options);
        }
    }
}
