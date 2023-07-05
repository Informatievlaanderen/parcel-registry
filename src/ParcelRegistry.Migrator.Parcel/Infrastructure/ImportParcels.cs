namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Importer.Grb.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using ParcelRegistry.Parcel;
    using ParcelRegistry.Parcel.Commands;

    internal sealed class ImportParcels
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly Dictionary<ParcelId, GrbParcel> _parcelGeometries;
        private readonly ILogger<ImportParcels> _logger;
        private readonly SqlStreamsTable _sqlStreamTable;

        public ImportParcels(
            ILifetimeScope lifetimeScope,
            IConfiguration configuration,
            Dictionary<Legacy.ParcelId, GrbParcel> parcelGeometries,
            ILoggerFactory loggerFactory)
        {
            _lifetimeScope = lifetimeScope;
            _parcelGeometries = parcelGeometries
                .Select(x => new KeyValuePair<ParcelId, GrbParcel>(ParcelId.CreateFor(new VbrCaPaKey(x.Value.GrbCaPaKey)), x.Value))
                .ToDictionary(x => x.Key, x => x.Value);

            var connectionString = configuration.GetConnectionString("events");
            _sqlStreamTable = new SqlStreamsTable(connectionString);

            _logger = loggerFactory.CreateLogger<ImportParcels>();
        }

        public async Task ImportNewParcels(CancellationToken cancellationToken = default)
        {
            var allStreamParcelIds = new HashSet<ParcelId>((await _sqlStreamTable.ReadAllNewStreamIds()).Select(x => new ParcelId(Guid.Parse(x.Split('-')[1]))));

            var newParcelIds = _parcelGeometries.Keys.Where(x => !allStreamParcelIds.Contains(x)).ToList();

            _logger.LogInformation($"Importing {newParcelIds.Count} new parcels.");
            foreach (var newParcelId in newParcelIds)
            {
                var grbParcel = _parcelGeometries[newParcelId];
                var command = new ImportParcel(
                    new VbrCaPaKey(grbParcel.GrbCaPaKey.VbrCaPaKey),
                    new ExtendedWkbGeometry(grbParcel.Geometry.AsBinary()),
                    new Provenance(
                        SystemClock.Instance.GetCurrentInstant(),
                        Application.ParcelRegistry,
                        new Reason("Import Parcel"),
                        new Operator("Parcel Registry"),
                        Modification.Insert,
                        Organisation.DigitaalVlaanderen));
                
                await using var scope = _lifetimeScope.BeginLifetimeScope();
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    command.CreateCommandId(),
                    command,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
