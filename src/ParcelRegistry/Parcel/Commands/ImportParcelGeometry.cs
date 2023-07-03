namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportParcelGeometry : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("23d7f186-ec16-4872-87b6-4d0efcc31976");

        public ParcelId ParcelId { get; }

        public VbrCaPaKey VbrCaPaKey { get; }

        public ExtendedWkbGeometry Geometry { get; }

        public Provenance Provenance { get; }

        public ImportParcelGeometry(
            VbrCaPaKey vbrCaPaKey,
            ExtendedWkbGeometry geometry,
            Provenance provenance)
        {
            VbrCaPaKey = vbrCaPaKey;
            ParcelId = ParcelId.CreateFor(VbrCaPaKey);
            Geometry = geometry;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportParcelGeometry-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return VbrCaPaKey;
            yield return Geometry;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}