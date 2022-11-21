using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Utilities;

namespace ParcelRegistry.Legacy.Commands
{
    public class MarkParcelAsMigrated : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("1a04e42e-8668-4526-bd89-abaa3bc251b7");

        public ParcelId ParcelId { get; }
        public Provenance Provenance { get; }

        public MarkParcelAsMigrated(
            ParcelId parcelId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MarkParcelAsMigrated-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
        }
    }
}
