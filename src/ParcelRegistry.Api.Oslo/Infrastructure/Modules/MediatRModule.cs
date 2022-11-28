namespace ParcelRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using MediatR;
    using Parcel.Handlers;
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
                builder.RegisterType<GetParcelListV2Handler>().AsImplementedInterfaces();
                builder.RegisterType<GetParcelV2Handler>().AsImplementedInterfaces();
                builder.RegisterType<GetParcelCountV2Handler>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<GetParcelListV1Handler>().AsImplementedInterfaces();
                builder.RegisterType<GetParcelV1Handler>().AsImplementedInterfaces();
                builder.RegisterType<GetParcelCountV1Handler>().AsImplementedInterfaces();
            }
        }
    }
}
