namespace ParcelRegistry.Projector.Importer
{
    using System;
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
            await using var sqlConnection =
                new SqlConnection(configuration.GetConnectionString(ImportGrbConnectionStringKey));
            var result =
                await sqlConnection.QueryFirstAsync<DateTimeOffset>(
                    @$"
                    SELECT TOP(1) [{nameof(RunHistory.ToDate)}]
                    FROM [{Schema.GrbImporter}].[{nameof(ImporterContext.RunHistory)}]
                    WHERE [{nameof(RunHistory.Completed)}] = 1
                    ORDER BY [{nameof(RunHistory.Id)}] DESC");

            return Ok(new[]
            {
                new
                {
                    Name = "Importer GRB",
                    LastProcessedMessage = result
                }
            });
        }
    }
}
