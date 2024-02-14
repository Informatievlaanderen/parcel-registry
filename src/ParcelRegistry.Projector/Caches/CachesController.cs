namespace ParcelRegistry.Projector.Caches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using ParcelRegistry.Projections.LastChangedList;
    using SqlStreamStore;

    [ApiVersion("1.0")]
    [ApiRoute("caches")]
    public class CachesController : ApiController
    {
        private static Dictionary<string, string> _projectionNameMapper = new Dictionary<string, string>()
        {
            {"ParcelRegistry.Projections.LastChangedList.LastChangedListProjections", LastChangedListProjections.ProjectionName}
        };

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IConfiguration configuration,
            [FromServices] LastChangedListContext lastChangedListContext,
            [FromServices] IReadonlyStreamStore streamStore,
            CancellationToken cancellationToken)
        {
            var maxErrorTimeInSeconds = configuration.GetValue<int?>("Caches:LastChangedList:MaxErrorTimeInSeconds") ?? 60;

            var maxErrorTime = DateTimeOffset.UtcNow.AddSeconds(-1 * maxErrorTimeInSeconds);
            var numberOfRecords = await lastChangedListContext
                .LastChangedList
                .OrderBy(x => x.Id)
                .Where(r => r.ToBeIndexed && (r.LastError == null || r.LastError < maxErrorTime))
                .CountAsync(cancellationToken);

            var positions = await lastChangedListContext.ProjectionStates.ToListAsync(cancellationToken);
            var streamPosition = await streamStore.ReadHeadPosition(cancellationToken);

            var response = new List<dynamic>
            {
                new
                {
                    name = "Cache detail percelen",
                    numberOfRecordsToProcess = numberOfRecords
                }
            };

            foreach (var position in positions)
            {
                response.Add(new
                {
                    name = _projectionNameMapper.ContainsKey(position.Name) ? _projectionNameMapper[position.Name] : position.Name,
                    numberOfRecordsToProcess = streamPosition - position.Position
                });
            }

            return Ok(response);
        }
    }
}
