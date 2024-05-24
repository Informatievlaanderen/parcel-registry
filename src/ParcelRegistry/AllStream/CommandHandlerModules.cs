namespace ParcelRegistry.AllStream
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<ProvenanceFactory<AllStream>>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder
                .RegisterType<AllStreamCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(AllStreamCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
