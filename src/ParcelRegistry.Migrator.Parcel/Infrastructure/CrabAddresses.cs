namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Dapper;
    using Legacy;
    using Microsoft.Data.SqlClient;
    using ParcelRegistry.Infrastructure;

    public sealed class ImportedCrabAdres
    {
        public string CaPaKey { get; set; }
        public int HuisNummerId { get; set; }
        public int? SubadresId { get; set; }

        public CrabAdres Map()
        {
            var vbrCaPaKey =
                new VbrCaPaKey(Be.Vlaanderen.Basisregisters.GrAr.Common.CaPaKey.CreateFrom(CaPaKey).VbrCaPaKey);
            var parcelId = ParcelId.CreateFor(vbrCaPaKey);

            return SubadresId is null
                ? new CrabAdres(parcelId, AddressId.CreateFor(new CrabHouseNumberId(HuisNummerId)))
                : new CrabAdres(parcelId, AddressId.CreateFor(new CrabSubaddressId(SubadresId.Value)));
        }
    }

    public struct CrabAdres
    {
        public Guid ParcelId { get; set; }
        public Guid AddressId { get; set; }

        public CrabAdres(Guid parcelId, Guid id)
        {
            ParcelId = parcelId;
            AddressId = id;
        }
    }

    public class CrabAddresses
    {
        private readonly string _connectionString;

        public CrabAddresses(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Dictionary<Guid, List<Guid>>> GetAddressesByParcel()
        {
            await using var conn = new SqlConnection(_connectionString);
            var sql =
@$"SELECT
[CaPaKey] as CaPaKey
,[huisNummerId] as HuisNummerId
,[subAdresId] as SubadresId
FROM [{Schema.MigrateParcel}].[CrabAddresses]";

            var importedCrabAdres = await conn.QueryAsync<ImportedCrabAdres>(sql);

            var addressesByParcel = importedCrabAdres
                .Select(x => x.Map())
                .GroupBy(x => x.ParcelId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.AddressId).ToList());

            return addressesByParcel;
        }
    }
}
