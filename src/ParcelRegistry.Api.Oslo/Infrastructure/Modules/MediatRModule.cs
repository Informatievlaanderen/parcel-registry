namespace ParcelRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Parcel.V2.Count;
    using Parcel.V2.Detail;
    using Parcel.V2.List;
    using Parcel.V2.Sync;
    using Module = Autofac.Module;
    using V3 = Parcel.V3;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ParcelListOsloV2Handler>().AsImplementedInterfaces();
            builder.RegisterType<ParcelDetailOsloV2Handler>().AsImplementedInterfaces();
            builder.RegisterType<ParcelCountOsloV2Handler>().AsImplementedInterfaces();
            builder.RegisterType<SyncHandler>().AsImplementedInterfaces();

            builder.RegisterType<V3.List.ParcelListOsloV3Handler>().AsImplementedInterfaces();
            builder.RegisterType<V3.Detail.ParcelDetailOsloV3Handler>().AsImplementedInterfaces();
            builder.RegisterType<V3.Count.ParcelCountOsloV3Handler>().AsImplementedInterfaces();
        }
    }
}
