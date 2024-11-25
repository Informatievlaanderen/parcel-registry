namespace ParcelRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.Testing.Infrastructure.Events;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.IO;
    using Parcel.Events;
    using Producer;
    using Producer.Snapshot.Oslo;
    using Projections.BackOffice;
    using Projections.Extract;
    using Projections.Extract.ParcelExtract;
    using Projections.Extract.ParcelLinkExtract;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.ParcelLatestItemV2;
    using Projections.Integration.ParcelVersion;
    using Projections.LastChangedList;
    using Projections.Legacy;
    using Projections.Legacy.ParcelDetail;
    using Projections.Legacy.ParcelSyndication;
    using SqlStreamStore;
    using Xunit;

    public sealed class ProjectionsHandlesEventsTests
    {
        private readonly IEnumerable<Type> _eventsToExclude = new[] { typeof(ParcelSnapshotV2) };
        private readonly IList<Type> _eventTypes;

        public ProjectionsHandlesEventsTests()
        {
            _eventTypes = DiscoverEventTypes();
        }

        private IList<Type> DiscoverEventTypes()
        {
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => InfrastructureEventsTests.GetAssemblyTypesSafe(a)
                    .Any(t => t.Name == "DomainAssemblyMarker"));

            if (domainAssembly == null)
            {
                return Enumerable.Empty<Type>().ToList();
            }

            return domainAssembly.GetTypes()
                .Where(t => t is { IsClass: true, Namespace: not null }
                            && IsEventNamespace(t)
                            && IsNotCompilerGenerated(t)
                            && t.GetCustomAttributes(typeof(EventNameAttribute), true).Length != 0)
                .Except(_eventsToExclude)
                .ToList();
        }

        private static bool IsEventNamespace(Type t) => t.Namespace?.EndsWith("Parcel.Events") ?? false;
        private static bool IsNotCompilerGenerated(MemberInfo t) => Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null;

        [Theory]
        [MemberData(nameof(GetProjectionsToTest))]
        public void ProjectionsHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest)
        {
            AssertHandleEvents(projectionsToTest);
        }

        public static IEnumerable<object[]> GetProjectionsToTest()
        {
            yield return [new List<ConnectedProjection<LegacyContext>>
            {
                new ParcelDetailProjections(),
                new ParcelSyndicationProjections(),
            }];

            yield return [new List<ConnectedProjection<LastChangedListContext>>
            {
                new LastChangedListProjections(Mock.Of<ICacheValidator>())
            }];

            yield return [new List<ConnectedProjection<IntegrationContext>>
            {
                new ParcelLatestItemV2Projections(Mock.Of<IOptions<IntegrationOptions>>()),
                new ParcelVersionProjections(Mock.Of<IAddressRepository>(), Mock.Of<IOptions<IntegrationOptions>>())
            }];

            yield return [new List<ConnectedProjection<ExtractContext>>
            {
                new ParcelExtractProjections(new OptionsWrapper<ExtractConfig>(new ExtractConfig()), Encoding.UTF8),
                new ParcelLinkExtractProjections(Encoding.UTF8)
            }];

            var inMemorySettings = new Dictionary<string, string>
            {
                { "DelayInSeconds", "10" }
            };
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            yield return [new List<ConnectedProjection<BackOfficeProjectionsContext>>
            {
                new BackOfficeProjections(Mock.Of<IDbContextFactory<BackOfficeContext>>(), configurationRoot)
            }];

            yield return [new List<ConnectedProjection<ParcelRegistry.Producer.Snapshot.Oslo.ProducerContext>>
            {
                new ProducerProjections(Mock.Of<IProducer>(), Mock.Of<ISnapshotManager>(), Mock.Of<IOsloProxy>())
            }];

            yield return [new List<ConnectedProjection<ParcelRegistry.Producer.ProducerContext>>
            {
                new ProducerMigrateProjections(Mock.Of<IProducer>())
            }];
        }

        private void AssertHandleEvents<T>(List<ConnectedProjection<T>> projectionsToTest, IList<Type>? eventsToExclude = null)
        {
            var eventsToCheck = _eventTypes.Except(eventsToExclude ?? Enumerable.Empty<Type>()).ToList();
            foreach (var projection in projectionsToTest)
            {
                projection.Handlers.Should().NotBeEmpty();
                foreach (var eventType in eventsToCheck)
                {
                    var messageType = projection.Handlers.Any(x => x.Message.GetGenericArguments().First() == eventType);
                    messageType.Should().BeTrue($"The event {eventType.Name} is not handled by the projection {projection.GetType().Name}");
                }
            }
        }
    }
}
