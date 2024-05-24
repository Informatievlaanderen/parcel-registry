namespace ParcelRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Legacy;
    using Microsoft.Extensions.Configuration;

    public class CommandHandlingModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new AggregateSourceModule(_configuration));

            CommandHandlerModules.Register(builder);
            ParcelRegistry.Parcel.CommandHandlerModules.Register(builder);
            AllStream.CommandHandlerModules.Register(builder);

            builder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
