namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Handlers;
    using Infrastructure;
    using Infrastructure.Download;

    public interface IRequestMapper
    {
        List<ParcelRequest> Map(Dictionary<GrbParcelActions, Stream> files);
    }

    public sealed class RequestMapper : IRequestMapper
    {
        public List<ParcelRequest> Map(Dictionary<GrbParcelActions, Stream> files)
        {
            var parcelsRequests = new List<ParcelRequest>();

            foreach (var (action, fileStream) in files)
            {
                switch (action)
                {
                    case GrbParcelActions.AddOrUpdate:
                        var parcels = new GrbXmlReader().Read(fileStream);
                        parcelsRequests.AddRange(parcels.Select(x => new ImportOrUpdateParcelRequest(x)));
                        break;
                    case GrbParcelActions.Delete:
                        parcelsRequests.AddRange(new GrbDeleteXmlReader().Read(fileStream).Select(x => new RetireParcelRequest(x)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"No {nameof(ParcelRequest)} mapping implemented for action {action}");
                }
            }

            return parcelsRequests;
        }
    }
}
