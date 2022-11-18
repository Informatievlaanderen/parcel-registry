namespace ParcelRegistry.Infrastructure.Modules
{
    using Autofac;
    using Parcel;
    using Repositories;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            builder
                .RegisterType<LegacyParcels>()
                .As<Legacy.IParcels>();

            builder
                .RegisterType<Parcels>()
                .As<IParcels>();
        }
    }
}
