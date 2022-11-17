namespace ParcelRegistry.Infrastructure.Modules
{
    using Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Legacy;
    using Microsoft.Extensions.Configuration;

    public class CommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new ParcelFactory(NoSnapshotStrategy.Instance))
                .As<IParcelFactory>();

            builder
                .RegisterModule<RepositoriesModule>();

            builder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            builder
                .RegisterEventstreamModule(_configuration);

            CommandHandlerModules.Register(builder);

            builder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
