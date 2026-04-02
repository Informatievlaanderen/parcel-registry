namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Dapper;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using Npgsql;

    public sealed class MunicipalityGeometryRepository : IMunicipalityGeometryRepository
    {
        private static readonly Instant CutoffDate = Instant.FromUtc(2025, 1, 1, 0, 0);

        private readonly string _connectionString;
        private List<MunicipalityGeometryItem>? _cachedGeometries2019;
        private List<MunicipalityGeometryItem>? _cachedGeometries;
        private readonly Lock _lock = new();

        public MunicipalityGeometryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex, Instant eventTimestamp)
        {
            EnsureCacheLoaded();

            var ewkbBytes = extendedWkbGeometryAsHex.ToByteArray();
            WKBReader? wkbReader = null;
            if (!ewkbBytes.TryReadSrid(out var srid))
            {
                srid = SystemReferenceId.SridLambert72;
                wkbReader = WKBReaderFactory.CreateForLambert72();
            }
            else
            {
                wkbReader = WKBReaderFactory.CreateForEwkb(ewkbBytes);
            }

            var parcelGeometry = wkbReader.Read(ewkbBytes);

            var geometries = eventTimestamp >= CutoffDate
                ? _cachedGeometries!
                : _cachedGeometries2019!;

            return geometries
                .Where(m => m.Srid == srid && m.Geometry.Intersects(parcelGeometry))
                .Select(m => m.NisCode)
                .Distinct()
                .ToList();
        }

        private void EnsureCacheLoaded()
        {
            if (_cachedGeometries is not null && _cachedGeometries2019 is not null)
                return;

            lock (_lock)
            {
                if (_cachedGeometries is not null && _cachedGeometries2019 is not null)
                    return;

                _cachedGeometries = LoadMunicipalityGeometries("integration_municipality.municipality_geometries");
                _cachedGeometries2019 = LoadMunicipalityGeometries("integration_municipality.municipality_geometries_2019");
            }
        }

        private List<MunicipalityGeometryItem> LoadMunicipalityGeometries(string tableName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = $@"SELECT nis_code, ST_AsBinary(geometry) as geometry, ST_AsBinary(geometry_lambert08) as geometry_lambert08
                         FROM {tableName}";

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
