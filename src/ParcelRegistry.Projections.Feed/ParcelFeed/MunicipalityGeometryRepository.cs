namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Dapper;
    using NetTopologySuite.Geometries;
    using Npgsql;

    public sealed class MunicipalityGeometryRepository : IMunicipalityGeometryRepository
    {
        private readonly string _connectionString;
        private List<MunicipalityGeometryItem>? _cachedGeometries;
        private readonly Lock _lock = new();

        public MunicipalityGeometryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex)
        {
            EnsureCacheLoaded();

            var ewkbBytes = extendedWkbGeometryAsHex.ToByteArray();
            var wkbReader = WKBReaderFactory.CreateForEwkb(ewkbBytes);
            var parcelGeometry = wkbReader.Read(ewkbBytes);
            var srid = parcelGeometry.SRID;

            return _cachedGeometries!
                .Where(m => m.Srid == srid && m.Geometry.Intersects(parcelGeometry))
                .Select(m => m.NisCode)
                .Distinct()
                .ToList();
        }

        private void EnsureCacheLoaded()
        {
            if (_cachedGeometries is not null)
                return;

            lock (_lock)
            {
                if (_cachedGeometries is not null)
                    return;

                _cachedGeometries = LoadMunicipalityGeometries();
            }
        }

        private List<MunicipalityGeometryItem> LoadMunicipalityGeometries()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"SELECT nis_code, geometry, geometry_lambert08
                        FROM integration_municipality.municipality_geometries";

            var rows = connection.Query(sql);

            var wkbReaderLambert72 = WKBReaderFactory.CreateForLambert72();
            var wkbReaderLambert2008 = WKBReaderFactory.CreateForLambert2008();

            var result = new List<MunicipalityGeometryItem>();

            foreach (var row in rows)
            {
                var nisCode = (string)row.nis_code;
                var geometryBytes = (byte[])row.geometry;
                var geometryLambert08Bytes = (byte[])row.geometry_lambert08;

                if (geometryBytes is { Length: > 0 })
                {
                    var geom = wkbReaderLambert72.Read(geometryBytes);
                    result.Add(new MunicipalityGeometryItem(nisCode, SystemReferenceId.SridLambert72, geom));
                }

                if (geometryLambert08Bytes is { Length: > 0 })
                {
                    var geom = wkbReaderLambert2008.Read(geometryLambert08Bytes);
                    result.Add(new MunicipalityGeometryItem(nisCode, SystemReferenceId.SridLambert2008, geom));
                }
            }

            return result;
        }

        private sealed record MunicipalityGeometryItem(string NisCode, int Srid, Geometry Geometry);
    }
}
