namespace ParcelRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Handlers;
    using MediatR;
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
               builder.RegisterType<GetParcelsV2Handler>().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<GetParcelsHandler>().AsImplementedInterfaces();
            }
        }
    }
}
