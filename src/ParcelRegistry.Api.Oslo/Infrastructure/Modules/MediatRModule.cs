namespace ParcelRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Parcel.Count;
    using Parcel.Detail;
    using Parcel.List;
    using Module = Autofac.Module;

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
        }
    }
}
