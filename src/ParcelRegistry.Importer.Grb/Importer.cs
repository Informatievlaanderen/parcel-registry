namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Infrastructure;
    using Infrastructure.Download;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;

    public sealed class Importer : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly IDownloadFacade _downloadFacade;
        private readonly IZipArchiveProcessor _zipArchiveProcessor;
        private readonly IRequestMapper _requestMapper;
        private readonly IDbContextFactory<ImporterContext> _importerContext;
        private readonly INotificationService _notificationService;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public Importer(
            IMediator mediator,
            IDownloadFacade downloadFacade,
            IZipArchiveProcessor zipArchiveProcessor,
            IRequestMapper requestMapper,
            IDbContextFactory<ImporterContext> importerContext,
            INotificationService notificationService,
            IHostApplicationLifetime applicationLifetime)
        {
            _mediator = mediator;
            _downloadFacade = downloadFacade;
            _zipArchiveProcessor = zipArchiveProcessor;
            _requestMapper = requestMapper;
            _importerContext = importerContext;
            _notificationService = notificationService;
            _applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var currentRun = await GetCurrentRunHistory(stoppingToken);

                var zipArchive = await _downloadFacade.Download(currentRun.FromDate, currentRun.ToDate);

                var files = _zipArchiveProcessor.Open(zipArchive);

                var parcelsRequests = _requestMapper.Map(files);

                var groupedParcels = parcelsRequests
                    .OrderBy(x => x.GrbParcel.VersionDate)
                    .ThenBy(x => x.GrbParcel.Version)
                    .GroupBy(y => y.GrbParcel.GrbCaPaKey);

                await ProcessRecords(stoppingToken, groupedParcels);

                await using var context = await _importerContext.CreateDbContextAsync(stoppingToken);
                await context.CompleteRunHistory(currentRun.Id);
                await context.ClearProcessedRequests();

                await _notificationService.PublishToTopicAsync(new NotificationMessage(
                    nameof(Grb),
                    $"Run completed: {currentRun.FromDate} - {currentRun.ToDate}",
                    "Parcel Importer Grb",
                    NotificationSeverity.Good));

                _applicationLifetime.StopApplication();
            }
            catch (OrderInvalidDateRangeException e)
            {
                throw;
            }
            catch (Exception e)
            {
                await _notificationService.PublishToTopicAsync(new NotificationMessage(
                    nameof(Grb),
                    e.Message,
                    "Parcel Importer Grb",
                    NotificationSeverity.Danger));
                throw;
            }
        }

        private async Task ProcessRecords(CancellationToken stoppingToken, IEnumerable<IGrouping<CaPaKey, ParcelRequest>> groupedParcels)
        {
            foreach (var parcelRequests in groupedParcels)
            {
                foreach (var request in parcelRequests)
                {
                    await using var context = await _importerContext.CreateDbContextAsync(stoppingToken);

                    try
                    {
                        if (await context.ProcessedRequestExists(request.Hash))
                        {
                            continue;
                        }

                        await _mediator.Send(request, stoppingToken);

                        await context.AddProcessedRequest(request.Hash);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Exception for parcel: {request.GrbParcel.GrbCaPaKey.VbrCaPaKey}, {e.GetType()} {Environment.NewLine} Parcel hash: {request.Hash}", e);
                    }
                }
            }
        }

        private async Task<RunHistory> GetCurrentRunHistory(CancellationToken stoppingToken)
        {
            await using var context = await _importerContext.CreateDbContextAsync(stoppingToken);

            var lastRun = await context.GetLatestRunHistory();
            RunHistory currentRun;
            if (lastRun.Completed)
            {
                var toDate = await _downloadFacade.GetMaxDate();
                var fromDate = lastRun.ToDate.AddDays(1);

                if (fromDate > toDate)
                {
                    throw new OrderInvalidDateRangeException($"{nameof(toDate)} must be greater than {nameof(fromDate)}.");
                }

                currentRun = await context.AddRunHistory(fromDate, toDate);
            }
            else
            {
                currentRun = lastRun;
            }

            return currentRun;
        }
    }

    public record ParcelRequest : IRequest
    {
        public GrbParcel GrbParcel { get; }

        public string Hash { get; }

        public ParcelRequest(GrbParcel grbParcel)
        {
            GrbParcel = grbParcel;
            Hash = this.GetSHA256();
        }
    }

    public static class GrbParcelRequestExtensions
    {
        public static string GetSHA256(this ParcelRequest request)
        {
            var stringToHash = request.GrbParcel.GrbCaPaKey + request.GrbParcel.Version + request.GrbParcel.Geometry;

            using var sha256 = SHA256.Create();

            var hashBytes = sha256.ComputeHash(ASCIIEncoding.ASCII.GetBytes(stringToHash));

            return hashBytes.ToHexString();
        }
    }
}
