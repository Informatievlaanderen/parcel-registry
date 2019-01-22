namespace ParcelRegistry.Infrastructure.Modules
{
    using Autofac;
    using Parcel;
    using Repositories;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            containerBuilder
                .RegisterType<Parcels>()
                .As<IParcels>();
        }
    }
}
