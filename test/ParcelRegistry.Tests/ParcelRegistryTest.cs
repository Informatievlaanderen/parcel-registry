namespace ParcelRegistry.Tests
{
    using System.Collections.Generic;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.Comparers;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Infrastructure.Modules;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Parcel;
    using Parcel.Events;
    using Xunit.Abstractions;

    public abstract class ParcelRegistryTest : AutofacBasedTest
    {
        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        protected ParcelRegistryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .Build();

            builder.RegisterModule(new CommandHandlingModule(configuration));
            builder
                .Register(c => new ParcelFactory(IntervalStrategy.SnapshotEvery(1)))
                .As<IParcelFactory>();

            builder.RegisterType<FixGrar1475ProvenanceFactory>().AsSelf();
            builder.RegisterType<FixGrar1637ProvenanceFactory>().AsSelf();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventSerializerSettings));

            var eventMappingDictionary =
                new Dictionary<string, System.Type>(
                    EventMapping.DiscoverEventNamesInAssembly(typeof(DomainAssemblyMarker).Assembly))
                {
                    {$"{nameof(SnapshotContainer)}<{nameof(ParcelSnapshot)}>", typeof(SnapshotContainer)}
                };

            builder.RegisterInstance(new EventMapping(eventMappingDictionary)).As<EventMapping>();
        }

        protected override IFactComparer CreateFactComparer()
        {
            var comparer = new CompareLogic();
            comparer.Config.MembersToIgnore.Add("Provenance");
            return new CompareNetObjectsBasedFactComparer(comparer);
        }

        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
