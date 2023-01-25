namespace ParcelRegistry.Tests.Legacy
{
    using System.Collections.Generic;
    using Autofac;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.Comparers;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using global::AutoFixture;
    using Infrastructure.Modules;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Parcel;
    using Xunit.Abstractions;
    using IParcelFactory = ParcelRegistry.Legacy.IParcelFactory;
    using ParcelFactory = ParcelRegistry.Legacy.ParcelFactory;

    public abstract class ParcelRegistryTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }

        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        protected ParcelRegistryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.Default);
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .Build();

            builder.RegisterModule(new SqlSnapshotStoreModule());
            builder.RegisterModule(new CommandHandlingModule(configuration));

            builder
                .Register(c => new FakeConsumerAddressContextFactory().CreateDbContext())
                .InstancePerLifetimeScope()
                .As<IAddresses>()
                .AsSelf();

            builder
                .Register(c => new ParcelFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IParcelFactory>();

            builder
                .Register(c => new ParcelRegistry.Parcel.ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()))
                .As<ParcelRegistry.Parcel.IParcelFactory>();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        { }

        protected override IFactComparer CreateFactComparer()
        {
            var comparer = new CompareLogic();
            comparer.Config.MembersToIgnore.Add("Provenance");
            return new CompareNetObjectsBasedFactComparer(comparer);
        }

        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
