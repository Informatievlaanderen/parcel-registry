namespace ParcelRegistry.Legacy
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using ParcelRegistry.Parcel;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<LegacyProvenanceFactory>()
                .SingleInstance();

            containerBuilder
                .RegisterType<ProvenanceFactory<Parcel>>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder.RegisterType<FixGrar1475ProvenanceFactory>().AsSelf();
            containerBuilder.RegisterType<FixGrar1637ProvenanceFactory>().AsSelf();
            containerBuilder.RegisterType<FixGrar3581ProvenanceFactory>().AsSelf();

            containerBuilder
                .RegisterType<ParcelCommandHandlerModule>()
                .Named<CommandHandlerModule>(typeof(ParcelCommandHandlerModule).FullName)
                .As<CommandHandlerModule>();
        }
    }
}
