namespace ParcelRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;

    /// <summary>
    /// Walks every parcel stream once and converts the geometry it holds from Lambert 72 (EPSG 31370) to
    /// Lambert 2008 (EPSG 3812). See ADR 0005.
    /// </summary>
    internal sealed class StreamConverter
    {
        private const string StreamIdPrefix = "parcel-";

        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;
        private readonly ProcessedStreamsTable _processedStreamsTable;
        private readonly SqlStreamsTable _sqlStreamsTable;
        private readonly bool _dryRun;
        private readonly int _maxDegreeOfParallelism;
        private readonly TimeSpan _slowStreamThreshold;
        private readonly int? _maxPagesPerRun;

        private readonly DurationStatistics _loadDurations = new();
        private readonly DurationStatistics _dispatchDurations = new();
        private readonly DurationStatistics _streamDurations = new();

        private HashSet<int> _processedIds = [];

        public StreamConverter(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ILifetimeScope lifetimeScope)
        {
            _logger = loggerFactory.CreateLogger("Lambert2008Converter");
            _lifetimeScope = lifetimeScope;

            var connectionString = configuration.GetConnectionString("Events")!;
            var pageSize = configuration.GetValue<int?>("PageSize") ?? 500;

            _processedStreamsTable = new ProcessedStreamsTable(connectionString, loggerFactory);
            _sqlStreamsTable = new SqlStreamsTable(connectionString, pageSize);

            _dryRun = configuration.GetValue<bool?>("DryRun") ?? true;
            _maxDegreeOfParallelism = configuration.GetValue<int?>("MaxDegreeOfParallelism") ?? 1;
            _slowStreamThreshold = TimeSpan.FromSeconds(
                configuration.GetValue<double?>("SlowStreamThresholdInSeconds") ?? 10);

            // Lets a run do a bounded amount of work and exit on its own, so evaluating in between does not
            // mean killing the process mid-page. Absent or 0 runs to the end.
            var maxPagesPerRun = configuration.GetValue<int?>("MaxPagesPerRun") ?? 0;
            _maxPagesPerRun = maxPagesPerRun > 0 ? maxPagesPerRun : null;
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            await _processedStreamsTable.CreateTableIfNotExists(ct);

            // Resume from the last fully completed page, then skip the tail of the page that was interrupted.
            // Streams within a page run in parallel, so a recorded high id says nothing about the ids below it
            // — only a completed page does.
            var lastCursorPosition = await _processedStreamsTable.GetResumeCursor(ct);
            _processedIds = (await _processedStreamsTable.GetProcessedIdsAfter(lastCursorPosition, ct)).ToHashSet();

            var totalStreams = await _sqlStreamsTable.CountParcelStreams(ct);
            var alreadyProcessed = await _processedStreamsTable.GetProcessedCount(ct);

            _logger.LogInformation(
                "Starting {Mode} over {TotalStreams} parcel streams, {AlreadyProcessed} already done, "
                + "resuming at internal id {Cursor} "
                + "({PartialPageStreams} streams of an interrupted page to skip), page size {PageSize}, "
                + "parallelism {Parallelism}, {PageLimit}.",
                _dryRun ? "DRY RUN" : "conversion",
                totalStreams,
                alreadyProcessed,
                lastCursorPosition,
                _processedIds.Count,
                _sqlStreamsTable.PageSize,
                _maxDegreeOfParallelism,
                _maxPagesPerRun is null ? "running to the end" : $"stopping after {_maxPagesPerRun} pages");

            if (_dryRun)
            {
                _logger.LogWarning(
                    "Running in dry run mode: streams are loaded and measured, but no commands are dispatched. "
                    + "Load timings are representative, dispatch timings are not.");
            }

            var run = Stopwatch.StartNew();
            var processedStreams = 0;
            var convertedParcels = 0;
            var pageNumber = 0;
            var stoppedEarly = false;

            var queryStarted = Stopwatch.GetTimestamp();
            var pageOfStreams = (await _sqlStreamsTable.ReadNextParcelStreamPage(lastCursorPosition)).ToList();
            var queryDuration = Stopwatch.GetElapsedTime(queryStarted);

            while (pageOfStreams.Count != 0)
            {
                pageNumber++;

                List<StreamConversionResult> pageResults;
                TimeSpan pageDuration;

                try
                {
                    var pageStarted = Stopwatch.GetTimestamp();
                    pageResults = await ProcessStreams(pageOfStreams, ct);
                    pageDuration = Stopwatch.GetElapsedTime(pageStarted);
                }
                catch (OperationCanceledException)
                {
                    // The streams that did finish are recorded, so the next run picks up mid-page. The page is
                    // deliberately not completed, which keeps the resume cursor behind it.
                    _logger.LogWarning("Cancelled during page {PageNumber}, stopping.", pageNumber);
                    stoppedEarly = true;
                    break;
                }

                await _processedStreamsTable.CompletePage(pageOfStreams.Select(x => x.internalId));

                processedStreams += pageResults.Count;
                convertedParcels += pageResults.Count(x => x.NeedsConversion);
                lastCursorPosition = pageOfStreams.Max(x => x.internalId);

                LogPage(
                    pageNumber,
                    pageOfStreams.Count,
                    pageResults,
                    pageDuration,
                    queryDuration,
                    alreadyProcessed + processedStreams,
                    totalStreams,
                    processedStreams,
                    convertedParcels,
                    run.Elapsed);

                if (pageNumber == _maxPagesPerRun)
                {
                    _logger.LogInformation(
                        "Reached the configured limit of {MaxPagesPerRun} pages, stopping at internal id {Cursor}.",
                        _maxPagesPerRun,
                        lastCursorPosition);
                    stoppedEarly = true;
                    break;
                }

                queryStarted = Stopwatch.GetTimestamp();
                pageOfStreams = (await _sqlStreamsTable.ReadNextParcelStreamPage(lastCursorPosition)).ToList();
                queryDuration = Stopwatch.GetElapsedTime(queryStarted);
            }

            run.Stop();

            LogSummary(
                run.Elapsed,
                alreadyProcessed + processedStreams,
                totalStreams,
                processedStreams,
                convertedParcels,
                lastCursorPosition,
                stoppedEarly);
        }

        private async Task<List<StreamConversionResult>> ProcessStreams(
            IEnumerable<(int internalId, string streamId)> streams,
            CancellationToken ct)
        {
            var results = new ConcurrentBag<StreamConversionResult>();

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = _maxDegreeOfParallelism
            };

            await Parallel.ForEachAsync(streams, parallelOptions, async (stream, innerCt) =>
            {
                var (internalId, streamId) = stream;

                if (_processedIds.Contains(internalId))
                {
                    _logger.LogDebug("Stream '{StreamId}' was already converted, skipping.", streamId);
                    return;
                }

                var parcelId = ParseParcelId(streamId);

                try
                {
                    var result = await ConvertStream(parcelId, innerCt);

                    _loadDurations.Add(result.LoadDuration);
                    _streamDurations.Add(result.TotalDuration);

                    // A dry run never dispatches, so recording its zeroes would report a dispatch
                    // percentile of 0ms instead of saying there is nothing to report.
                    if (!_dryRun)
                    {
                        _dispatchDurations.Add(result.DispatchDuration);
                    }

                    await _processedStreamsTable.Add(internalId, parcelId, result);

                    LogStream(streamId, result);

                    results.Add(result);
                }
                catch (Exception exception)
                {
                    _logger.LogCritical(
                        exception,
                        "Unexpected exception converting stream '{StreamId}' (internal id {InternalId}).",
                        streamId,
                        internalId);
                    throw;
                }
            });

            return [.. results];
        }

        private async Task<StreamConversionResult> ConvertStream(Guid parcelId, CancellationToken ct)
        {
            await using var streamLifetimeScope = _lifetimeScope.BeginLifetimeScope();

            var loadStarted = Stopwatch.GetTimestamp();

            var parcels = streamLifetimeScope.Resolve<IParcels>();
            var parcel = await parcels.GetAsync(new ParcelStreamId(new ParcelId(parcelId)), ct);

            // A parcel holds one geometry, so unlike the address conversion there is nothing to count per
            // stream. Its size is recorded instead: polygon complexity is what makes one parcel cost more
            // than another, and it is the thing a staging run has to extrapolate against.
            var geometryBytes = parcel.Geometry?.ToString().ToByteArray() ?? [];
            var needsConversion = geometryBytes.Length != 0 && !IsLambert2008(geometryBytes);

            var loadDuration = Stopwatch.GetElapsedTime(loadStarted);
            var geometryByteCount = geometryBytes.Length;

            if (!needsConversion || _dryRun)
            {
                return new StreamConversionResult(
                    needsConversion,
                    geometryByteCount,
                    loadDuration,
                    TimeSpan.Zero);
            }

            var dispatchStarted = Stopwatch.GetTimestamp();

            var command = new TransformToLambert2008(
                new ParcelId(parcelId),
                CreateProvenance());

            // A fresh scope per dispatch: the command handler resolves its own unit of work, and reusing the
            // scope the aggregate was read in would append against a stale one.
            await using (var dispatchScope = _lifetimeScope.BeginLifetimeScope())
            {
                await dispatchScope
                    .Resolve<ICommandHandlerResolver>()
                    .Dispatch(command.CreateCommandId(), command, cancellationToken: ct);
            }

            return new StreamConversionResult(
                needsConversion,
                geometryByteCount,
                loadDuration,
                Stopwatch.GetElapsedTime(dispatchStarted));
        }

        private static Provenance CreateProvenance()
            => new Provenance(
                SystemClock.Instance.GetCurrentInstant(),
                Application.ParcelRegistry,
                new Reason("Omzetting van de perceelgeometrie naar Lambert 2008 (EPSG 3812)."),
                new Operator(string.Empty),
                Modification.Update,
                Organisation.DigitaalVlaanderen);

        private void LogStream(string streamId, StreamConversionResult result)
        {
            const string message =
                "Stream '{StreamId}': {GeometryByteCount} geometry bytes, needs conversion {NeedsConversion}, took "
                + "{TotalMilliseconds}ms (load {LoadMilliseconds}ms, dispatch {DispatchMilliseconds}ms).";

            // Individual streams are far too many to log at Information, but the slow ones are exactly what a
            // staging run is looking for, so those are raised regardless of level.
            var level = result.TotalDuration >= _slowStreamThreshold ? LogLevel.Warning : LogLevel.Debug;

            _logger.Log(
                level,
                message,
                streamId,
                result.GeometryByteCount,
                result.NeedsConversion,
                (int)result.TotalDuration.TotalMilliseconds,
                (int)result.LoadDuration.TotalMilliseconds,
                (int)result.DispatchDuration.TotalMilliseconds);
        }

        private void LogPage(
            int pageNumber,
            int pageStreamCount,
            IReadOnlyCollection<StreamConversionResult> pageResults,
            TimeSpan pageDuration,
            TimeSpan queryDuration,
            int doneStreams,
            int totalStreams,
            int processedThisRun,
            int convertedParcels,
            TimeSpan elapsed)
        {
            var streamsPerSecond = pageDuration.TotalSeconds > 0
                ? pageStreamCount / pageDuration.TotalSeconds
                : 0;

            _logger.LogInformation(
                "Page {PageNumber}: {PageStreamCount} streams ({ProcessedInPage} processed, {ParcelsInPage} to convert) "
                + "in {PageSeconds:F1}s at {StreamsPerSecond:F1} streams/s, paging query {QueryMilliseconds}ms. "
                + "Progress {DoneStreams}/{TotalStreams} ({ProgressPercentage:F1}%), {ConvertedParcels} parcels "
                + "this run, elapsed {Elapsed:g}, ETA {EstimatedTimeLeft}.",
                pageNumber,
                pageStreamCount,
                pageResults.Count,
                pageResults.Count(x => x.NeedsConversion),
                pageDuration.TotalSeconds,
                streamsPerSecond,
                (int)queryDuration.TotalMilliseconds,
                doneStreams,
                totalStreams,
                totalStreams == 0 ? 0 : 100d * doneStreams / totalStreams,
                convertedParcels,
                elapsed,
                DescribeTimeLeft(elapsed, doneStreams, totalStreams, processedThisRun));

            LogDurations("Per stream so far");
        }

        /// <summary>
        /// Extrapolated from this run's rate over the whole run so far, rather than from the last page, so one
        /// heavy page does not swing it. It assumes the remaining streams look like the ones already done, and
        /// it covers the whole job — including what earlier runs did — not just what is left of this run.
        /// </summary>
        private static string DescribeTimeLeft(TimeSpan elapsed, int doneStreams, int totalStreams, int processedThisRun)
        {
            if (processedThisRun == 0)
            {
                return "unknown";
            }

            var remainingStreams = Math.Max(totalStreams - doneStreams, 0);
            var timeLeft = TimeSpan.FromTicks(elapsed.Ticks / processedThisRun * remainingStreams);

            return timeLeft.ToString("g", CultureInfo.InvariantCulture);
        }

        private void LogSummary(
            TimeSpan elapsed,
            int doneStreams,
            int totalStreams,
            int processedThisRun,
            int convertedParcels,
            int cursor,
            bool stoppedEarly)
        {
            var streamsPerSecond = elapsed.TotalSeconds > 0 ? processedThisRun / elapsed.TotalSeconds : 0;

            _logger.LogInformation(
                "{Outcome} after {Elapsed:g}: {ProcessedThisRun} streams processed this run, "
                + "{ConvertedParcels} parcels {Verb}, at {StreamsPerSecond:F1} streams/s. "
                + "{DoneStreams}/{TotalStreams} parcels done overall, next run resumes at internal id {Cursor}.",
                stoppedEarly ? "Stopped" : "Finished",
                elapsed,
                processedThisRun,
                convertedParcels,
                _dryRun ? "to convert" : "converted",
                streamsPerSecond,
                doneStreams,
                totalStreams,
                cursor);

            LogDurations("Per stream");
        }

        private void LogDurations(string prefix)
            => _logger.LogInformation(
                prefix + ": total {StreamDurations} | load {LoadDurations} | dispatch {DispatchDurations}.",
                _streamDurations.Describe(),
                _loadDurations.Describe(),
                _dispatchDurations.Describe());

        private static bool IsLambert2008(byte[] extendedWkbGeometry)
            => extendedWkbGeometry.TryReadSrid(out var srid) && srid == SystemReferenceId.SridLambert2008;

        private static Guid ParseParcelId(string streamId)
        {
            if (!streamId.StartsWith(StreamIdPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Stream '{streamId}' is not a parcel stream.");
            }

            return Guid.Parse(streamId[StreamIdPrefix.Length..]);
        }
    }
}
