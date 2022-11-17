namespace ParcelRegistry.Infrastructure.Modules
{
    using Autofac;
    using Legacy;
    using Repositories;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            builder
                .RegisterType<Parcels>()
                .As<IParcels>();
        }
    }
}
