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

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var ctx = context.Resolve<IComponentContext>();
                return type => ctx.Resolve(type);
            });


            if (_useProjectionsV2)
            {
                builder.RegisterType<ParcelListOsloV2Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelDetailOsloV2Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelCountOsloV2Handler>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<ParcelListOsloV1Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelDetailOsloV1Handler>().AsImplementedInterfaces();
                builder.RegisterType<ParcelCountOsloV1Handler>().AsImplementedInterfaces();
            }
        }
    }
}
