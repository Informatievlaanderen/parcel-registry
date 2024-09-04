namespace ParcelRegistry.Api.Oslo.Parcel.Sync
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using ParcelRegistry.Projections.Legacy;

    public class SyncHandler : IRequestHandler<SyncRequest, string>
    {
        private readonly IConfiguration _configuration;
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public SyncHandler(
            IConfiguration configuration,
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _configuration = configuration;
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<string> Handle(SyncRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<ParcelSyndicationFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var lastFeedUpdate = await _context
                .ParcelSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var pagedParcels = new ParcelSyndicationQuery(
                    _context,
                    filtering.Filter?.Embed)
                .Fetch(filtering, sorting, pagination);

            return await BuildAtomFeed(lastFeedUpdate, pagedParcels, _responseOptions, _configuration);
        }

        private static async Task<string> BuildAtomFeed(
            DateTimeOffset lastUpdate,
            PagedQueryable<ParcelSyndicationQueryResult> pagedParcels,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");
                var atomConfiguration = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastUpdate);

                await writer.WriteDefaultMetadata(atomConfiguration);

                var parcels = pagedParcels.Items.ToList();
                var nextFrom = parcels.Any()
                    ? parcels.Max(x => x.Position) + 1
                    : (long?)null;

                var nextUri = BuildNextSyncUri(pagedParcels.PaginationInfo.Limit, nextFrom, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, "next"));

                foreach (var parcel in pagedParcels.Items)
                    await writer.WriteParcel(responseOptions, formatter, syndicationConfiguration["Category"], parcel);

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri BuildNextSyncUri(int limit, long? from, string nextUrlBase)
        {
            return from.HasValue
                ? new Uri(string.Format(nextUrlBase, from, limit))
                : null;
        }
    }
}
