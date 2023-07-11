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
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;

    public interface IUniqueParcelPlanProxy
    {
        public Task<Stream> Download(DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken);
    }

    public interface IZipArchiveProcessor
    {
        public Task<Dictionary<GrbParcelActions, FileStream>> Open(Stream stream, CancellationToken cancellationToken);
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

        public Importer(IMediator mediator, IUniqueParcelPlanProxy uniqueParcelPlanProxy, IZipArchiveProcessor zipArchiveProcessor)
        {
            _mediator = mediator;
            _uniqueParcelPlanProxy = uniqueParcelPlanProxy;
            _zipArchiveProcessor = zipArchiveProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // first run
            //db DateTime.lowest --> lastDateTime
            // now --> vandaag

            // second (volgende dag)
            // LastDT -> gisteren
            // now

            var stream = await _uniqueParcelPlanProxy.Download(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), stoppingToken);

            var files = await _zipArchiveProcessor.Open(stream, stoppingToken);

            var parcelsRequests = new List<GrbParcelRequest>();

            foreach (var (action, fileStream) in files)
            {
                switch (action)
                {
                    case GrbParcelActions.Add:
                        var parcels = new GrbAddXmlReader().Read(fileStream);
                        parcelsRequests.AddRange(parcels.Select(x => new GrbAddParcelRequest(x)));
                        break;
                    case GrbParcelActions.Update:
                        parcelsRequests.AddRange(new GrbUpdateXmlReader().Read(fileStream).Select(x => new GrbAddParcelRequest(x)));

                        break;
                    case GrbParcelActions.Delete:
                        parcelsRequests.AddRange(new GrbDeleteXmlReader().Read(fileStream).Select(x => new GrbAddParcelRequest(x)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var groupedParcels = parcelsRequests
                .OrderBy(x => x.GrbParcel.Version)
                .GroupBy(y => y.GrbParcel.GrbCaPaKey);

            foreach (var parcelRequest in groupedParcels.SelectMany(x => x))
            {
                try
                {
                    // get hash
                    // if same skip
                    await _mediator.Send(parcelRequest, stoppingToken);
                    // context.Insert(parcel);
                    // update
                    var hash = parcelRequest.GetSHA256();

                }
                catch (DomainException e)
                { }
                catch (Exception e)
                { }
            }
        }
    }

    public record GrbParcelRequest(GrbParcel GrbParcel) : IRequest<ParcelResponse>;

    public record ParcelResponse;

    public static class GrbParcelRequestExtensions
    {
        public static string GetSHA256(this GrbParcelRequest request)
        {
            var stringToHash = request.GrbParcel.GrbCaPaKey + request.GrbParcel.Version + request.GrbParcel.Geometry;

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stringToHash.ToByteArray());

            return hashBytes.ToHexString();
        }
    }
}
