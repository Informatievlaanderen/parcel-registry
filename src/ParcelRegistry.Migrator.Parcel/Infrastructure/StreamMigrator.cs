namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System;
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
    using ParcelRegistry.Parcel;
    using Consumer.Address;
    using Legacy.Commands;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Serilog;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    internal class StreamMigrator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;
        private readonly ProcessedIdsTable _processedIdsTable;
        private readonly SqlStreamsTable _sqlStreamTable;
        private Dictionary<Guid, int> _consumedAddressItems;

        private List<(int processedId, bool isPageCompleted)> _processedIds;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public StreamMigrator(ILoggerFactory loggerFactory, IConfiguration configuration, ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            _logger = loggerFactory.CreateLogger("ParcelMigrator");

            var connectionString = configuration.GetConnectionString("events");
            _processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            _sqlStreamTable = new SqlStreamsTable(connectionString);
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            await _processedIdsTable.CreateTableIfNotExists();

            var consumerAddressContext = _lifetimeScope.Resolve<ConsumerAddressContext>();
            _consumedAddressItems = await consumerAddressContext
                .AddressConsumerItems
                .Where(x => x.AddressId != null)
                .Select(x => new { AddressId = x.AddressId!.Value, x.AddressPersistentLocalId })
                .ToDictionaryAsync(x => x.AddressId, y => y.AddressPersistentLocalId, ct);

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

            while (pageOfStreams.Any())
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

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
            var processedItems = new List<int>();

            foreach (var stream in streamsToProcess)
            {
                try
                {
                    await Policy
                        .Handle<SqlException>()
                        .WaitAndRetryAsync(10,
                            currentRetry => Math.Pow(currentRetry, 2) * TimeSpan.FromSeconds(30),
                            (_, timespan) =>
                                Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                        .ExecuteAsync(async () =>
                        {
                            await ProcessStream(stream, processedItems, ct);
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        $"Unexpected exception for migration stream '{stream.Item1}', aggregateId '{stream.Item2}' \n\n {ex.Message}");
                    throw;
                }
            }

            return processedItems;
        }

        private async Task ProcessStream(
            (int, string) stream,
            List<int> processedItems,
            CancellationToken ct)
        {
            var (internalId, aggregateId) = stream;

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (_processedIds.Contains((internalId, false)))
            {
                _logger.LogDebug($"Already migrated '{internalId}', skipping...");
                return;
            }

            await using var streamLifetimeScope = _lifetimeScope.BeginLifetimeScope();

            var legacyParcelsRepo = streamLifetimeScope.Resolve<Legacy.IParcels>();
            var parcelId = new Legacy.ParcelId(Guid.Parse(aggregateId));

            _stopwatch.Start();
            var legacyParcelAggregate = await legacyParcelsRepo.GetAsync(parcelId, ct);
            _stopwatch.Stop();
            _logger.LogInformation("Resolved aggregate in {timing}", _stopwatch.Elapsed.ToString("g", CultureInfo.InvariantCulture));
            _stopwatch.Reset();

            if (legacyParcelAggregate.IsRemoved)
            {
                _logger.LogDebug($"Skipping removed parcel '{aggregateId}'.");
                return;
            }

            var migrateParcel = legacyParcelAggregate.CreateMigrateCommand(addressId =>
            {
                if (!_consumedAddressItems.TryGetValue(addressId, out var addressPersistentLocalId))
                {
                    throw new InvalidOperationException($"AddressConsumerItem for addressId '{addressId}' was not found in the ConsumerAddressContext.");
                }

                return new AddressPersistentLocalId(addressPersistentLocalId);
            });
            var markMigrated = new MarkParcelAsMigrated(
                new Legacy.ParcelId(migrateParcel.ParcelId),
                migrateParcel.Provenance);

            await DispatchCommand(markMigrated, ct);
            await DispatchCommand(migrateParcel, ct);

            await _processedIdsTable.Add(internalId);
            processedItems.Add(internalId);

            await using var backOfficeContext = streamLifetimeScope.Resolve<BackOfficeContext>();
            foreach (var addressPersistentLocalId in migrateParcel.AddressPersistentLocalIds)
            {
                await backOfficeContext
                    .ParcelAddressRelations.AddAsync(
                        new ParcelAddressRelation(
                            migrateParcel.ParcelId,
                            addressPersistentLocalId), ct);
            }
            await backOfficeContext.SaveChangesAsync(ct);
        }

        private async Task DispatchCommand<TCommand>(
            TCommand command,
            CancellationToken ct)
        where TCommand : IHasCommandProvenance
        {
            await using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    command.CreateCommandId(),
                    command,
                    cancellationToken: ct);
            }
        }
    }
}
