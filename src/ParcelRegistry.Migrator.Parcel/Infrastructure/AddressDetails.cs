namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public sealed class AddressDetails
    {
        private readonly string _connectionString;

        public AddressDetails(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Dictionary<Guid, int>> GetActualAddresses()
        {
            await using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<QueryObject>($@"
SELECT v1.AddressId AS AddressId, v2.AddressPersistentLocalId AS PersistentLocalId
FROM [AddressRegistryLegacy].[AddressDetailsV2] v2
INNER JOIN [AddressRegistryLegacy].[AddressDetails] v1
    ON v2.AddressPersistentLocalId = v1.PersistentLocalId
    AND v2.Removed = 0
    AND (v2.Status = 1 OR v2.status = 2)");

            return result.ToDictionary(x => x.AddressId, y => y.PersistentLocalId);
        }

        private sealed class QueryObject
        {
            public Guid AddressId { get; set; }
            public int PersistentLocalId { get; set; }
        }
    }
}
