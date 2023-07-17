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

        public Importer(
            IMediator mediator,
            IDownloadFacade downloadFacade,
            IZipArchiveProcessor zipArchiveProcessor,
            IRequestMapper requestMapper,
            IDbContextFactory<ImporterContext> importerContext,
            INotificationService notificationService)
        {
            _mediator = mediator;
            _downloadFacade = downloadFacade;
            _zipArchiveProcessor = zipArchiveProcessor;
            _requestMapper = requestMapper;
            _importerContext = importerContext;
            _notificationService = notificationService;
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
                    .OrderBy(x => x.GrbParcel.Version)
                    .GroupBy(y => y.GrbParcel.GrbCaPaKey);

                await ProcessRecords(stoppingToken, groupedParcels);

                await using var context = await _importerContext.CreateDbContextAsync(stoppingToken);
                await context.CompleteRunHistory(currentRun.Id);
                await context.ClearProcessedRequests();
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
            foreach (var parcelRequest in groupedParcels.SelectMany(x => x))
            {
                await using var context = await _importerContext.CreateDbContextAsync(stoppingToken);
                try
                {
                    if (await context.ProcessedRequestExists(parcelRequest.Hash))
                    {
                        continue;
                    }

                    await _mediator.Send(parcelRequest, stoppingToken);

                    await context.AddProcessedRequest(parcelRequest.Hash);
                }
                catch (Exception e)
                {
                    throw new Exception($"Exception for parcel: {parcelRequest.GrbParcel.GrbCaPaKey.VbrCaPaKey}, {e.GetType()} {Environment.NewLine} Parcel hash: {parcelRequest.Hash}", e);
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
                var maxDate = await _downloadFacade.GetMaxDate();
                currentRun = await context.AddRunHistory(lastRun.ToDate, maxDate);
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
