namespace ParcelRegistry.Projector.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
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
    using ParcelRegistry.Projections.LastChangedList;
    using ParcelRegistry.Projections.Legacy;
    using ParcelRegistry.Projections.Legacy.ParcelDetail;
    using ParcelRegistry.Projections.Legacy.ParcelSyndication;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly UseProjectionsV2Toggle _useProjectionsV2Toggle;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            UseProjectionsV2Toggle useProjectionsV2Toggle)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
            _useProjectionsV2Toggle = useProjectionsV2Toggle;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new DataDogModule(_configuration));
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

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

            if (_useProjectionsV2Toggle.FeatureEnabled)
            {
                RegisterExtractV2Projections(builder);
                // RegisterLastChangedV2Projections(builder);
                // RegisterLegacyV2Projections(builder);
            }
            else
            {
                RegisterExtractProjections(builder);
                RegisterLastChangedProjections(builder);
                RegisterLegacyProjections(builder);
            }
        }

        private void RegisterExtractProjections(ContainerBuilder builder)
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
                .RegisterProjections<ParcelExtractProjections, ExtractContext>(
                    context => new ParcelExtractProjections(context.Resolve<IOptions<ExtractConfig>>(),
                        DbaseCodePage.Western_European_ANSI.ToEncoding()), ConnectedProjectionSettings.Default);
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

            builder
                .RegisterProjectionMigrator<
                    ParcelRegistry.Projections.LastChangedList.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjectionMigrator<DataMigrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<LastChangedListProjections, LastChangedListContext>(ConnectedProjectionSettings
                    .Default);
        }

        private void RegisterLegacyProjections(ContainerBuilder builder)
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
                .RegisterProjections<ParcelDetailProjections, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<ParcelSyndicationProjections, LegacyContext>(ConnectedProjectionSettings.Default);
        }
    }
}
