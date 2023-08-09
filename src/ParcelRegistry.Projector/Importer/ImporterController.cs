namespace ParcelRegistry.Projector.Importer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using ParcelRegistry.Importer.Grb;
    using ParcelRegistry.Infrastructure;

    [ApiVersion("1.0")]
    [ApiRoute("importer")]
    public class ConsumersController : ApiController
    {
        private const string? ImportGrbConnectionStringKey = "ImporterGrb";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var connectionString = configuration.GetConnectionString(ImportGrbConnectionStringKey);
            var lastCompletedRun = await GetRunHistory(true, connectionString);
            var currentRun = await GetRunHistory(false, connectionString);

            return Ok(
                new
                {
                    Name = "Importer GRB",
                    LastCompletedBatch = new { From = lastCompletedRun.FromDate, Until = lastCompletedRun.ToDate },
                    CurrentBatch = currentRun is not null ? new { From = currentRun.FromDate, Until = currentRun.ToDate } : null
                }
            );
        }

        private async Task<RunHistory?> GetRunHistory(bool completed, string connectionString)
        {
            await using var sqlConnection = new SqlConnection(connectionString);
            var runHistory = await sqlConnection.QueryFirstOrDefaultAsync<RunHistory>(
                    @$"
                    SELECT TOP(1) *
                    FROM [{Schema.GrbImporter}].[{nameof(ImporterContext.RunHistory)}]
                    WHERE [{nameof(RunHistory.Completed)}] = @completed
                    ORDER BY [{nameof(RunHistory.Id)}] DESC", new {completed});

            return runHistory;
        }
    }
}
