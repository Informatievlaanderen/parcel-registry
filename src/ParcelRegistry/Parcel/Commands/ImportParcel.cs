namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportParcel : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("23d7f186-ec16-4872-87b6-4d0efcc31976");

        public ParcelId ParcelId { get; }

        public VbrCaPaKey VbrCaPaKey { get; }

        public ExtendedWkbGeometry ExtendedWkbGeometry { get; }

        public Provenance Provenance { get; }

        public ImportParcel(
            VbrCaPaKey vbrCaPaKey,
            ExtendedWkbGeometry extendedWkbGeometry,
            Provenance provenance)
        {
            VbrCaPaKey = vbrCaPaKey;
            ParcelId = ParcelId.CreateFor(VbrCaPaKey);
            ExtendedWkbGeometry = extendedWkbGeometry;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportParcel-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
            yield return VbrCaPaKey;
            yield return ExtendedWkbGeometry;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
