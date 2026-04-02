namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System.Collections.Generic;

    public interface IMunicipalityGeometryRepository
    {
        List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex);
    }
}
