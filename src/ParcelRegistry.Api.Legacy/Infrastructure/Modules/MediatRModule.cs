namespace ParcelRegistry.Api.Legacy.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Parcel.Count;
    using Parcel.List;
    using Parcel.Sync;
    using ParcelRegistry.Api.Legacy.Parcel.Detail;
    using Module = Autofac.Module;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SyncHandler>().AsImplementedInterfaces();

            builder.RegisterType<ParcelDetailV2Handler>().AsImplementedInterfaces();
            builder.RegisterType<ParcelListV2Handler>().AsImplementedInterfaces();
            builder.RegisterType<ParcelCountV2Handler>().AsImplementedInterfaces();
        }
    }
}
