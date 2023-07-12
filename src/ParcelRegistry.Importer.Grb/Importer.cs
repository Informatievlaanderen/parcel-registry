namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Intrinsics.Arm;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Handlers;
    using Infrastructure;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using System.Security.Cryptography;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;

    public interface IUniqueParcelPlanProxy
    {
        public Task<Stream> Download(DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken);
        DateTimeOffset GetMaxDate();
    }

    public class UniqueParcelPlanProxy : IUniqueParcelPlanProxy
    {
        public Task<Stream> Download(DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetMaxDate()
        {
            throw new NotImplementedException();
        }
    }

    public interface IZipArchiveProcessor
    {
        public Task<Dictionary<GrbParcelActions, FileStream>> Open(Stream stream, CancellationToken cancellationToken);
    }

    public class ZipArchiveProcessor : IZipArchiveProcessor
    {
        public Task<Dictionary<GrbParcelActions, FileStream>> Open(Stream stream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public enum GrbParcelActions
    {
        Add,
        Update,
        Delete
    }

    public class Importer : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly IUniqueParcelPlanProxy _uniqueParcelPlanProxy;
        private readonly IZipArchiveProcessor _zipArchiveProcessor;
        private readonly IRequestMapper _requestMapper;
        private readonly IImporterContext _importerContext;

        public Importer(
            IMediator mediator,
            IUniqueParcelPlanProxy uniqueParcelPlanProxy,
            IZipArchiveProcessor zipArchiveProcessor,
            IRequestMapper requestMapper,
            IImporterContext importerContext)
        {
            _mediator = mediator;
            _uniqueParcelPlanProxy = uniqueParcelPlanProxy;
            _zipArchiveProcessor = zipArchiveProcessor;
            _requestMapper = requestMapper;
            _importerContext = importerContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var lastRun = await _importerContext.GetLatestRunHistory();
            RunHistory currentRun;
            if (lastRun.Completed)
            {
               currentRun = await _importerContext.AddRunHistory(lastRun.ToDate, _uniqueParcelPlanProxy.GetMaxDate());
            }
            else
            {
                currentRun = lastRun;
            }

            var stream = await _uniqueParcelPlanProxy.Download(currentRun.FromDate, currentRun.ToDate, stoppingToken);

            var files = await _zipArchiveProcessor.Open(stream, stoppingToken);

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
                catch (DomainException e)
                { }
                catch (Exception e)
                { }
            }

            // update history
            await _importerContext.CompleteRunHistory(currentRun.Id);
            await _importerContext.ClearProcessedRequests();
        }
    }

    public record ParcelRequest(GrbParcel GrbParcel) : IRequest;

    public record ParcelResponse;

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
