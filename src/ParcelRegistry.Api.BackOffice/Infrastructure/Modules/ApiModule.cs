namespace ParcelRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Consumer.Address;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Parcel;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Infrastructure.Modules;
    using Validators;

    public class ApiModule : Module
    {
        internal const string SqsQueueUrlConfigKey = "SqsQueueUrl";
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
            builder
                .RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder
                .RegisterType<IfMatchHeaderValidator>()
                .As<IIfMatchHeaderValidator>()
                .AsSelf();

            builder
                .RegisterType<ParcelExistsValidator>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterModule(new IdempotencyModule(
                _services,
                _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>()
                    .ConnectionString,
                new IdempotencyMigrationsTableInfo(Schema.Import),
                new IdempotencyTableInfo(Schema.Import),
                _loggerFactory));

            builder
                .RegisterModule(new EnvelopeModule())
                .RegisterModule(new BackOfficeModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new EditModule(_configuration))
                .RegisterModule(new MediatRModule())
                .RegisterModule(new SqsHandlersModule(_configuration[SqsQueueUrlConfigKey]))
                .RegisterModule(new TicketingModule(_configuration, _services))
                .RegisterModule(new ConsumerAddressModule(_configuration, _services, _loggerFactory))
                .RegisterSnapshotModule(_configuration);

            builder.Populate(_services);
        }
    }
}
