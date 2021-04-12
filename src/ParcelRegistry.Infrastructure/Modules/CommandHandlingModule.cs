namespace ParcelRegistry.Infrastructure.Modules
{
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Microsoft.Extensions.Configuration;
    using Parcel;

    public class CommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .Register(c => new ParcelFactory(IntervalStrategy.Default))
                .As<IParcelFactory>();

            containerBuilder
                .RegisterModule<RepositoriesModule>();

            containerBuilder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterEventstreamModule(_configuration);

            CommandHandlerModules.Register(containerBuilder);

            containerBuilder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
