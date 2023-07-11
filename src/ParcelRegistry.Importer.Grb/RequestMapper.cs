namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Handlers;
    using Infrastructure;

    public interface IRequestMapper
    {
        List<GrbParcelRequest> Map(Dictionary<GrbParcelActions, FileStream> files);
    }

    public class RequestMapper: IRequestMapper
    {
        public List<GrbParcelRequest> Map(Dictionary<GrbParcelActions, FileStream> files)
        {
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

            return parcelsRequests;
        }
    }
}
