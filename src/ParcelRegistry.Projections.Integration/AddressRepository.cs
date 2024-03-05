namespace ParcelRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Dapper;
    using Npgsql;

    public interface IAddressRepository
    {
        Task<int?> GetAddressPersistentLocalId(Guid addressId);
    }

    public class AddressRepository : IAddressRepository
    {
        private readonly string _connectionString;

        public AddressRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int?> GetAddressPersistentLocalId(Guid addressId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT persistent_local_id
	                    FROM integration_address.address_id_address_persistent_local_id
	                    WHERE address_id = @AddressId;";

            return await connection.QuerySingleOrDefaultAsync<int?>(sql, new
            {
                AddressId = addressId
            });
        }
    }
}
