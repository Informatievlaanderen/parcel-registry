namespace ParcelRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Handlers;
    using MediatR;
    using Module = Autofac.Module;

    public class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<GetParcelsV2Handler>().AsImplementedInterfaces();
            builder.RegisterType<GetParcelLinksHandler>().AsImplementedInterfaces();
        }
    }
}
