namespace ParcelRegistry.Projector.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using ParcelRegistry.Infrastructure;
    using ParcelRegistry.Projections.Extract;
    using ParcelRegistry.Projections.Extract.ParcelExtract;
    using ParcelRegistry.Projections.Extract.ParcelLinkExtract;
    using ParcelRegistry.Projections.Integration;
    using ParcelRegistry.Projections.Integration.Infrastructure;
    using ParcelRegistry.Projections.Integration.ParcelLatestItem;
    using ParcelRegistry.Projections.Integration.ParcelVersion;
    using ParcelRegistry.Projections.LastChangedList;
    using ParcelRegistry.Projections.Legacy;
    using ParcelRegistry.Projections.Legacy.ParcelDetailV2;
    using ParcelRegistry.Projections.Legacy.ParcelSyndication;

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
            builder.RegisterModule(new DataDogModule(_configuration));
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Register(c => new CacheValidator(c.Resolve<LegacyContext>(), typeof(ParcelDetailV2Projections).FullName))
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterModule<EnvelopeModule>()
                .RegisterEventStreamModule(_configuration)
                .RegisterModule(new ProjectorModule(_configuration));

            RegisterLastChangedProjections(builder);
            RegisterExtractV2Projections(builder);
            RegisterLegacyV2Projections(builder);

            if(_configuration.GetSection("Integration").GetValue("Enabled", false))
                RegisterIntegrationProjections(builder);
        }

        private void RegisterExtractV2Projections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ParcelExtractV2Projections, ExtractContext>(
                    context => new ParcelExtractV2Projections(
                        context.Resolve<IOptions<ExtractConfig>>(),
                        DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<ParcelLinkExtractProjections, ExtractContext>(
                    context => new ParcelLinkExtractProjections(
                        context.Resolve<IOptions<ExtractConfig>>(),
                        DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ParcelLastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _configuration["DataDog:ServiceName"],
                    _services,
                    _loggerFactory));
        }

        private void RegisterLegacyV2Projections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ParcelDetailV2Projections, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<ParcelSyndicationProjections, LegacyContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterIntegrationProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new IntegrationModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<IntegrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ParcelLatestItemProjections, IntegrationContext>(
                    context => new ParcelLatestItemProjections(context.Resolve<IOptions<IntegrationOptions>>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<ParcelVersionProjections, IntegrationContext>(
                    context => new ParcelVersionProjections(
                        context.Resolve<IAddressRepository>(),
                        context.Resolve<IOptions<IntegrationOptions>>()),
                    ConnectedProjectionSettings.Default);
        }
    }
}
