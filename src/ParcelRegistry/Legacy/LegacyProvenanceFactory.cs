namespace ParcelRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands.Fixes;
    using NodaTime;

    public class LegacyProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Parcel>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCrabProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Parcel aggregate)
        {
            if (!(provenanceHolder is IHasCrabProvenance crabProvenance))
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return CreateFrom(
                aggregate.LastModificationBasedOnCrab,
                crabProvenance.Timestamp,
                crabProvenance.Modification,
                crabProvenance.Operator,
                crabProvenance.Organisation);
        }
    }

    public class FixGrar1475ProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Parcel>
    {
        public bool CanCreateFrom<TCommand>() => typeof(FixGrar1475).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Parcel aggregate)
        {
            if (!(provenanceHolder is FixGrar1475))
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return new Provenance(Instant.FromDateTimeUtc(DateTime.UtcNow), Application.Unknown, new Reason("Rechtzetting adressen verwijderde percelen"), new Operator("crabadmin"), Modification.Delete, Organisation.Aiv);
        }
    }

    public class FixGrar1637ProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Parcel>
    {
        public bool CanCreateFrom<TCommand>() => typeof(FixGrar1637).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Parcel aggregate)
        {
            if (!(provenanceHolder is FixGrar1637))
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return new Provenance(Instant.FromDateTimeUtc(DateTime.UtcNow), Application.Unknown, new Reason("Rechtzetting herstelde percelen"), new Operator("crabadmin"), Modification.Insert, Organisation.Aiv);
        }
    }

    public class FixGrar3581ProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Parcel>
    {
        public bool CanCreateFrom<TCommand>() => typeof(FixGrar3581).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(object provenanceHolder, Parcel aggregate)
        {
            if (!(provenanceHolder is FixGrar3581))
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return new Provenance(Instant.FromDateTimeUtc(DateTime.UtcNow), Application.Unknown, new Reason("Rechtzetting staat percelen"), new Operator("crabadmin"), Modification.Update, Organisation.DigitaalVlaanderen);
        }
    }
}
