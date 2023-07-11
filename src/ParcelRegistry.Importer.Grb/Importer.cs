namespace ParcelRegistry.Importer.Grb
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Hosting;

    public interface IUniqueParcelPlanProxy
    {

    }

    public class Importer : BackgroundService
    {
        private readonly IMediator _mediator;
        private readonly IUniqueParcelPlanProxy _uniqueParcelPlanProxy;

        public Importer(IMediator mediator, IUniqueParcelPlanProxy uniqueParcelPlanProxy)
        {
            _mediator = mediator;
            _uniqueParcelPlanProxy = uniqueParcelPlanProxy;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_uniqueParcelPlanProxy.Download()
            // create ziparchive from stream
            // use xml readers to deserialize data
            // concat all records into one collection
            // order by capakey + version
            // for each record trigger Handler
        }
    }
}
