namespace ParcelRegistry.Parcel
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public class ParcelProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Parcel>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCrabProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Parcel aggregate)
        {
            if (!(provenanceHolder is IHasCrabProvenance crabProvenance))
                throw new ApplicationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");

            return CreateFrom(
                aggregate.LastModificationBasedOnCrab,
                crabProvenance.Timestamp,
                crabProvenance.Modification,
                crabProvenance.Operator,
                crabProvenance.Organisation);
        }
    }
}
