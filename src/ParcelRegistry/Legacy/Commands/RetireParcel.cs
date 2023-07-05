namespace ParcelRegistry.Legacy.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RetireParcel  : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("877b0c3e-bab7-46e8-a069-2201b4d67f8d");

        public ParcelId ParcelId { get; }
        public Provenance Provenance { get; }

        public RetireParcel(
            ParcelId parcelId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireParcel-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
        }
    }
}
