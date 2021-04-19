namespace ParcelRegistry.Tests.AutoFixture
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using global::AutoFixture;
    using global::AutoFixture.Dsl;
    using global::AutoFixture.Kernel;
    using NodaTime;
    using Parcel.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public class InfrastructureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize(new NodaTimeCustomization());
            fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.Default);
        }
    }

    public class SetProvenanceImplementationsCallSetProvenance : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var provenanceEventTypes = typeof(DomainAssemblyMarker).Assembly
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace != null && t.Namespace.EndsWith("Events") && t.GetInterfaces().Any(i => i == typeof(ISetProvenance)))
                .ToList();

            foreach (var allEventType in provenanceEventTypes)
            {
                var getSetProvenanceMethod = GetType()
                    .GetMethod("GetSetProvenance", BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(allEventType);
                var setProvenanceDelegate = getSetProvenanceMethod.Invoke(this, new object[] { fixture.Create<Provenance>() });

                var customizeMethod = typeof(Fixture).GetMethods().Single(m => m.Name == "Customize" && m.IsGenericMethod);
                var genericCustomizeMethod = customizeMethod.MakeGenericMethod(allEventType);
                genericCustomizeMethod.Invoke(fixture, new object[] { setProvenanceDelegate });
            }
        }

        private Func<ICustomizationComposer<T>, ISpecimenBuilder> GetSetProvenance<T>(Provenance provenance)
            where T : ISetProvenance
        {
            return c => c.Do(@event =>
                (@event as ISetProvenance).SetProvenance(provenance));
        }
    }

    public class NodaTimeCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new LocalDateGenerator());
            fixture.Customizations.Add(new LocalTimeGenerator());
            fixture.Customizations.Add(new LocalDateTimeGenerator());
        }

        public class LocalDateGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalDate).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalDate.FromDateTime(DateTime.Today);
            }
        }

        public class LocalTimeGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalTime).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalTime.FromTicksSinceMidnight(DateTime.Now.TimeOfDay.Ticks);
            }
        }

        public class LocalDateTimeGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!typeof(LocalDateTime).Equals(request))
                {
                    return new NoSpecimen();
                }

                return LocalDateTime.FromDateTime(DateTime.Now);
            }
        }
    }
}
