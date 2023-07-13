﻿namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Infrastructure;
    using Infrastructure.Download;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Parcel.Exceptions;

    public class Importer : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly IUniqueParcelPlanProxy _uniqueParcelPlanProxy;
        private readonly IZipArchiveProcessor _zipArchiveProcessor;
        private readonly IRequestMapper _requestMapper;
        private readonly IImporterContext _importerContext;
        private readonly INotificationService _notificationService;

        public Importer(
            IMediator mediator,
            IUniqueParcelPlanProxy uniqueParcelPlanProxy,
            IZipArchiveProcessor zipArchiveProcessor,
            IRequestMapper requestMapper,
            IImporterContext importerContext,
            INotificationService notificationService)
        {
            _mediator = mediator;
            _uniqueParcelPlanProxy = uniqueParcelPlanProxy;
            _zipArchiveProcessor = zipArchiveProcessor;
            _requestMapper = requestMapper;
            _importerContext = importerContext;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var lastRun = await _importerContext.GetLatestRunHistory();
                RunHistory currentRun;
                if (lastRun.Completed)
                {
                    var maxDate = await _uniqueParcelPlanProxy.GetMaxDate();
                    currentRun = await _importerContext.AddRunHistory(lastRun.ToDate, maxDate);
                }
                else
                {
                    currentRun = lastRun;
                }

                var zipArchive = await _uniqueParcelPlanProxy.Download(currentRun.FromDate, currentRun.ToDate);

                var files = _zipArchiveProcessor.Open(zipArchive);

                var parcelsRequests = _requestMapper.Map(files);

                var groupedParcels = parcelsRequests
                    .OrderBy(x => x.GrbParcel.Version)
                    .GroupBy(y => y.GrbParcel.GrbCaPaKey);

                foreach (var parcelRequest in groupedParcels.SelectMany(x => x))
                {
                    try
                    {
                        if (await _importerContext.ProcessedRequestExists(parcelRequest.GetSHA256()))
                        {
                            continue;
                        }

                        await _mediator.Send(parcelRequest, stoppingToken);

                        await _importerContext.AddProcessedRequest(parcelRequest.GetSHA256());
                    }
                    catch (Exception e)
                    {
                        throw new ImportGrbException($"Exception for parcel: {parcelRequest.GrbParcel.GrbCaPaKey.VbrCaPaKey}, {e.GetType()}", e);
                    }
                }

                await _importerContext.CompleteRunHistory(currentRun.Id);
                await _importerContext.ClearProcessedRequests();
            }
            catch (ImportGrbException e)
            {
                await _notificationService.PublishToTopicAsync(new NotificationMessage(
                    nameof(ParcelRegistry.Importer.Grb),
                    e.Message,
                    "Parcel Importer Grb",
                    NotificationSeverity.Danger));
                throw;
            }
            catch (Exception e)
            {
                await _notificationService.PublishToTopicAsync(new NotificationMessage(
                    nameof(ParcelRegistry.Importer.Grb),
                    e.Message,
                    "Parcel Importer Grb",
                    NotificationSeverity.Danger));
                throw;
            }
        }
    }

    public record ParcelRequest(GrbParcel GrbParcel) : IRequest;

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
