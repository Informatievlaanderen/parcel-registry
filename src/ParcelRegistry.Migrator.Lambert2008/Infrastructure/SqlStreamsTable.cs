namespace ParcelRegistry.Migrator.Lambert2008.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using ParcelRegistry.Infrastructure;

    /// <summary>
    /// Reads the parcel streams to convert, paged on the streams table's own internal id so the conversion
    /// can be resumed where it left off.
    /// </summary>
    internal sealed class SqlStreamsTable
    {
        private readonly string _connectionString;
        private readonly int _pageSize;

        public SqlStreamsTable(string connectionString, int pageSize = 500)
        {
            _connectionString = connectionString;
            _pageSize = pageSize;
        }

        public int PageSize => _pageSize;

        /// <summary>
        /// Every geometry the event store holds lives in a parcel stream, so those are the complete set of
        /// streams to convert. The legacy parcel streams are left untouched.
        /// </summary>
        public async Task<IEnumerable<(int internalId, string streamId)>> ReadNextParcelStreamPage(int lastCursorPosition)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<(int, string)>($"""
                                                               select top (@PageSize)
                                                                   [IdInternal]
                                                                   ,[IdOriginal]
                                                               from
                                                                   [{Schema.Default}].[Streams]
                                                               where
                                                                   IdOriginal like 'parcel-%'
                                                                   and IdInternal > @LastCursorPosition
                                                               order by
                                                                   IdInternal
                                                               """, new { PageSize = _pageSize, LastCursorPosition = lastCursorPosition }, commandTimeout: 60);
        }

        /// <summary>
        /// The total up front, so progress can be reported as a percentage with an estimate of the time
        /// left instead of an ever-growing count. One scan at startup, not per page.
        /// </summary>
        public async Task<int> CountParcelStreams(CancellationToken ct)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(new CommandDefinition($@"
select
    count(*)
from
    [{Schema.Default}].[Streams]
where
    IdOriginal like 'parcel-%'", commandTimeout: 300, cancellationToken: ct));
        }
    }
}
