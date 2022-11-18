namespace ParcelRegistry.Tests.Legacy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
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
    using ParcelRegistry.Legacy;
    using Xunit.Abstractions;

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

            builder.RegisterModule(new CommandHandlingModule(configuration));
            builder
                .Register(c => new ParcelFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IParcelFactory>();

            builder.RegisterModule(new SqlSnapshotStoreModule());

            builder
                .Register(c => new ParcelRegistry.Parcel.ParcelFactory(NoSnapshotStrategy.Instance))
                .As<ParcelRegistry.Parcel.IParcelFactory>();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var types =
                (from t in typeof(DomainAssemblyMarker).Assembly.GetTypes().AsParallel()
                 let attributes = t.GetCustomAttributes(typeof(EventNameAttribute), true)
                 where attributes != null && attributes.Length == 1
                 select new { Type = t, EventName = attributes.Cast<EventNameAttribute>().Single().Value }).ToList();
            var x = (from t in typeof(DomainAssemblyMarker).Assembly.GetTypes().AsParallel()
                     let attributes = t.GetCustomAttributes(typeof(EventSnapshotAttribute), true)
                     where attributes != null && attributes.Length == 1
                     select new
                     {
                         Type = attributes.Cast<EventSnapshotAttribute>().Single().SnapshotType,
                         EventName = attributes.Cast<EventSnapshotAttribute>().Single().EventName
                     }).ToList();

            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, EventSerializerSettings));
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
