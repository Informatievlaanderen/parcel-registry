namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ParcelRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public sealed class SqlStreamsTable
    {
        private readonly string _connectionString;
        private readonly int _pageSize;

        public SqlStreamsTable(string connectionString, int pageSize = 500)
        {
            _connectionString = connectionString;
            _pageSize = pageSize;
        }

        public async Task<IEnumerable<(int internalId, string aggregateId)>> ReadNextParcelStreamPage(int lastCursorPosition)
        {
            await using var conn = new SqlConnection(_connectionString);

            return await conn.QueryAsync<(int, string)>($@"
select top ({_pageSize})
	[IdInternal]
    ,[IdOriginal]
from
    [{Schema.Default}].[Streams]
where
    IdOriginal not like 'parcel-%'
    and IdInternal > {lastCursorPosition}
order by
    IdInternal", commandTimeout: 60);
        }

        public async Task<IEnumerable<string>> ReadAllNewStreamIds()
        {
            await using var conn = new SqlConnection(_connectionString);

            return await conn.QueryAsync<string>($@"
select [IdOriginal]
from
    [{Schema.Default}].[Streams]
where
    IdOriginal like 'parcel-%'
order by
    IdInternal", commandTimeout: 600);
        }
    }
}
