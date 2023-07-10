namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Importer.Grb.Infrastructure;
    using Legacy.Commands;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using ParcelRegistry.Parcel;
    using Polly;
    using Serilog;
    using ILogger = Microsoft.Extensions.Logging.ILogger;
    using IParcels = Legacy.IParcels;
    using ParcelId = Legacy.ParcelId;

    internal class StreamMigrator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;
        private readonly ProcessedIdsTable _processedIdsTable;
        private readonly SqlStreamsTable _sqlStreamTable;
        private readonly Dictionary<Guid, int> _consumedAddressItems;
        private readonly Dictionary<Guid, List<Guid>> _addressesByParcel;

        private readonly bool _skipNotFoundAddress;

        private List<(int processedId, bool isPageCompleted)> _processedIds;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Dictionary<ParcelId, GrbParcel> _parcelGeometriesByParcelId;

        public StreamMigrator(ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ILifetimeScope lifetimeScope,
            Dictionary<Guid, int> consumedAddressItems,
            Dictionary<Guid, List<Guid>> addressesByParcel,
            Dictionary<ParcelId, GrbParcel> parcelGeometries)
        {
            _lifetimeScope = lifetimeScope;
            _logger = loggerFactory.CreateLogger("ParcelMigrator");

            var connectionString = configuration.GetConnectionString("events");
            _processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            _sqlStreamTable = new SqlStreamsTable(connectionString);
            _consumedAddressItems = consumedAddressItems;
            _addressesByParcel = addressesByParcel;
            _parcelGeometriesByParcelId = parcelGeometries;

            _skipNotFoundAddress = bool.Parse(configuration["SkipNotFoundAddress"]);
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            await _processedIdsTable.CreateTableIfNotExists();

            var processedIdsList = await _processedIdsTable.GetProcessedIds();
            _processedIds = new List<(int, bool)>(processedIdsList);

            var lastCursorPosition = _processedIds.Any()
                ? _processedIds
                    .Where(x => x.isPageCompleted)
                    .Select(x => x.processedId)
                    .DefaultIfEmpty(0)
                    .Max()
                : 0;

            var pageOfStreams = (await _sqlStreamTable.ReadNextParcelStreamPage(lastCursorPosition)).ToList();

            while (pageOfStreams.Any() && !ct.IsCancellationRequested)
            {
                try
                {
                    var processedPageItems = await ProcessStreams(pageOfStreams, ct);

                    if (!processedPageItems.Any())
                    {
                        lastCursorPosition = pageOfStreams.Max(x => x.internalId);
                    }
                    else
                    {
                        await _processedIdsTable.CompletePageAsync(pageOfStreams.Select(x => x.internalId).ToList());
                        processedPageItems.ForEach(x => _processedIds.Add((x, true)));
                        lastCursorPosition = _processedIds.Max(x => x.processedId);
                    }

                    pageOfStreams = (await _sqlStreamTable.ReadNextParcelStreamPage(lastCursorPosition)).ToList();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ProcessStreams cancelled.");
                }
            }
        }

        private async Task<List<int>> ProcessStreams(IEnumerable<(int, string)> streamsToProcess, CancellationToken ct)
        {
            var processedItems = new ConcurrentBag<int>();

            await Parallel.ForEachAsync(streamsToProcess, ct, async (stream, innerCt) =>
            {
                try
                {
                    await Policy
                        .Handle<SqlException>()
                        .WaitAndRetryAsync(10,
                            currentRetry => Math.Pow(currentRetry, 2) * TimeSpan.FromSeconds(30),
                            (_, timespan) =>
                                Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                        .ExecuteAsync(async () => { await ProcessStream(stream, processedItems, innerCt); });
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        $"Unexpected exception for migration stream '{stream.Item1}', aggregateId '{stream.Item2}' \n\n {ex.Message}");
                    throw;
                }
            });

            return processedItems.ToList();
        }

        private async Task ProcessStream(
            (int, string) stream,
            ConcurrentBag<int> processedItems,
            CancellationToken ct)
        {
            var (internalId, aggregateId) = stream;

            if (_processedIds.Contains((internalId, false)))
            {
                _logger.LogDebug($"Already migrated '{internalId}', skipping...");
                return;
            }

            await using var streamLifetimeScope = _lifetimeScope.BeginLifetimeScope();

            var legacyParcelsRepo = streamLifetimeScope.Resolve<IParcels>();
            var parcelId = new ParcelId(Guid.Parse(aggregateId));

            _stopwatch.Start();
            var legacyParcelAggregate = await legacyParcelsRepo.GetAsync(parcelId, ct);
            _stopwatch.Stop();
            _logger.LogInformation("Resolved aggregate in {timing}",
                _stopwatch.Elapsed.ToString("g", CultureInfo.InvariantCulture));
            _stopwatch.Reset();

            if (legacyParcelAggregate.IsRemoved)
            {
                _logger.LogDebug($"Skipping removed parcel '{aggregateId}'.");
                return;
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            var addressIds = _addressesByParcel.ContainsKey(parcelId)
                ? _addressesByParcel[parcelId]
                    .Select(addressId =>
                    {
                        if (_consumedAddressItems.TryGetValue(addressId, out var addressPersistentLocalId))
                        {
                            return (isSuccess: true, addressPersistentLocalId: new AddressPersistentLocalId(addressPersistentLocalId));
                        }

                        if (_skipNotFoundAddress)
                        {
                            return (isSuccess: false, addressPersistentLocalId: new AddressPersistentLocalId(-1));
                        }

                        throw new InvalidOperationException(
                            $"AddressConsumerItem for addressId '{addressId}' was not found in the ConsumerAddressContext.");
                    })
                : Array.Empty<(bool, AddressPersistentLocalId)>();

            if (_parcelGeometriesByParcelId.TryGetValue(parcelId, out var grbParcel))
            {
                var migrateParcel = legacyParcelAggregate.CreateMigrateCommand(addressIds
                        .Where(x => x.isSuccess)
                        .Select(x => x.addressPersistentLocalId)
                        .ToList(),
                    new ExtendedWkbGeometry(grbParcel.Geometry.ToBinary()));

                var markMigrated = new MarkParcelAsMigrated(
                    migrateParcel.OldParcelId,
                    migrateParcel.Provenance);

                await DispatchCommand(markMigrated, CancellationToken.None);
                await DispatchCommand(migrateParcel, CancellationToken.None);

                await _processedIdsTable.Add(internalId);
                processedItems.Add(internalId);

                await using var backOfficeContext = streamLifetimeScope.Resolve<BackOfficeContext>();
                foreach (var addressPersistentLocalId in migrateParcel.AddressPersistentLocalIds)
                {
                    await backOfficeContext
                        .ParcelAddressRelations.AddAsync(
                            new ParcelAddressRelation(
                                migrateParcel.NewParcelId,
                                addressPersistentLocalId), CancellationToken.None);
                }

                await backOfficeContext.SaveChangesAsync(CancellationToken.None);
            }
            else
            {
                _logger.LogWarning($"Parcel geometry not found for '{aggregateId}', retiring parcel.");

                var provenance = new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Migrate Parcel aggregate."),
                    new Operator("Parcel Registry"),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                await DispatchCommand(new RetireParcel(parcelId, provenance), CancellationToken.None);
                await DispatchCommand(new MarkParcelAsMigrated(parcelId, provenance), CancellationToken.None); //TODO: do we mark it or not?

                await _processedIdsTable.Add(internalId);
                processedItems.Add(internalId);
            }
        }

        private async Task DispatchCommand<TCommand>(
            TCommand command,
            CancellationToken ct)
            where TCommand : IHasCommandProvenance
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope();
            var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
            await cmdResolver.Dispatch(
                command.CreateCommandId(),
                command,
                cancellationToken: ct);
        }
    }
}
