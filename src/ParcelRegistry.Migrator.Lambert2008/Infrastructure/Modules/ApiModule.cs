namespace ParcelRegistry.Migrator.Lambert2008.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Parcel;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Infrastructure.Modules;

    public sealed class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _configuration = configuration;
            _services = services;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new CommandHandlingModule(_configuration));

            builder.RegisterSnapshotModule(_configuration);

            // The parcel factory takes an IAddresses, but only the attach and detach paths ever call it, and the
            // conversion touches neither. Registering it as unreachable rather than wiring up the consumer's
            // address context keeps this job off a database it has no business reading.
            builder
                .RegisterType<UnreachableAddresses>()
                .As<IAddresses>();

            builder.Populate(_services);
        }
    }
}
