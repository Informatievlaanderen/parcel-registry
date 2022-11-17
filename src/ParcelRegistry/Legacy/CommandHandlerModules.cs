namespace ParcelRegistry.Legacy
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<ParcelProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<ParcelCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(ParcelCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
