namespace ParcelRegistry.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Microsoft.Extensions.Configuration;
    using Parcel;

    public class AggregateSourceModule : Module
    {
        public const string SnapshotIntervalKey = "SnapshotInterval";

        private readonly IConfiguration _configuration;

        public AggregateSourceModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var value = _configuration[SnapshotIntervalKey] ?? "50";
            var snapshotInterval = Convert.ToInt32(value);

            ISnapshotStrategy snapshotStrategy = NoSnapshotStrategy.Instance;
            if (snapshotInterval > 0)
            {
                snapshotStrategy = IntervalStrategy.SnapshotEvery(snapshotInterval);
            }

            builder
                .Register(c => new Legacy.ParcelFactory(NoSnapshotStrategy.Instance))
                .As<Legacy.IParcelFactory>();

            builder
                .Register(c => new ParcelFactory(snapshotStrategy, c.Resolve<IAddresses>()))
                .As<IParcelFactory>();

            builder
                .RegisterModule<RepositoriesModule>()
                .RegisterEventStreamModule(_configuration);

            builder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventsJsonSerializerSettingsProvider.CreateSerializerSettings()));
        }
    }
}
