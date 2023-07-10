namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ChangeParcelGeometry : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("628b502c-0eb7-4e45-9e86-ca52bf233311");

        public ParcelId ParcelId { get; }

        public VbrCaPaKey VbrCaPaKey { get; }

        public ExtendedWkbGeometry ExtendedWkbGeometry { get; }

        public Provenance Provenance { get; }

        public ChangeParcelGeometry(
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
            => Deterministic.Create(Namespace, $"ChangeParcelGeometry-{ToString()}");

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
