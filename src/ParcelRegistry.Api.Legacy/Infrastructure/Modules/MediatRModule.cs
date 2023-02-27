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
        private readonly bool _useProjectionsV2;

        public MediatRModule(bool useProjectionsV2)
        {
            _useProjectionsV2 = useProjectionsV2;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SyncHandler>().AsImplementedInterfaces();

            if (_useProjectionsV2)
            {
                builder.RegisterType<ParcelDetailV2Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelListV2Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelCountV2Handler>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<ParcelDetailV1Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelListV1Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelCountV1Handler>().AsImplementedInterfaces();
            }
        }
    }
}
