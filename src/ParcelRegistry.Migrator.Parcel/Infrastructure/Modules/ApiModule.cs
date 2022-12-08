namespace ParcelRegistry.Migrator.Parcel.Infrastructure.Modules
{
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Consumer.Address.Infrastructure.Modules;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Infrastructure.Modules;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var backOfficeConnectionString = _configuration.GetConnectionString("BackOffice");
            _services
                .AddDbContext<BackOfficeContext>(options => options
                        .UseLoggerFactory(_loggerFactory)
                        .UseSqlServer(backOfficeConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice))
                    , ServiceLifetime.Transient);

            builder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule(new EditModule(_configuration))
                .RegisterModule(new ConsumerAddressModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory))
                .RegisterEventStreamModule(_configuration);

            builder.Populate(_services);
        }
    }
}
