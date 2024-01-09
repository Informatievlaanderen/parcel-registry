namespace ParcelRegistry.Parcel
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NodaTime;

    public class ProvenanceFactory<TAggregateRoot> : IProvenanceFactory<TAggregateRoot>
        where TAggregateRoot : IAggregateRootEntity
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCommandProvenance).IsAssignableFrom(typeof(TCommand));
        public Provenance CreateFrom(object provenanceHolder, TAggregateRoot aggregate)
        {
            if (provenanceHolder is not IHasCommandProvenance provenance)
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return new Provenance(
                SystemClock.Instance.GetCurrentInstant(),
                provenance.Provenance.Application,
                provenance.Provenance.Reason,
                provenance.Provenance.Operator,
                provenance.Provenance.Modification,
                provenance.Provenance.Organisation);
        }
    }
}
