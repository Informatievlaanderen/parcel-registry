namespace ParcelRegistry.Tests
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Fixtures;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Parcel;
    using BackOffice;
    using Xunit.Abstractions;

    public class ParcelRegistryTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }
        protected string ConfigDetailUrl => "http://base/{0}";
        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        public ParcelRegistryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();

            Fixture.Customize(new WithParcelStatus());

            Fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            Fixture.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Snapshots", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "DetailUrl", ConfigDetailUrl } })
                .Build();

            builder.Register(a => (IConfiguration)configuration);

            builder
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());

            builder.RegisterModule(new SqlSnapshotStoreModule());

            builder
                .Register(c => new FakeConsumerAddressContextFactory().CreateDbContext())
                .InstancePerLifetimeScope()
                .As<IAddresses>()
                .AsSelf();

            builder
                .Register(c => new ParcelFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IParcelFactory>();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }

        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
