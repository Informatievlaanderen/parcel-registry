namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System.Collections.Generic;
    using NodaTime;

    public interface IMunicipalityGeometryRepository
    {
        List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex, Instant eventTimestamp);
    }
}
