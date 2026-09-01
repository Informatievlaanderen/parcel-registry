namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    /// <summary>
    /// Transforms the geometry the parcel holds to Lambert 2008 (EPSG 3812), see ADR 0005. It takes a
    /// <see cref="ParcelId"/> and nothing else: the transformation has nothing to decide per parcel.
    /// </summary>
    public class TransformToLambert2008 : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("6d1c9f42-77b3-4ba6-9e57-8c0a1d2b3e40");

        public ParcelId ParcelId { get; }

        public Provenance Provenance { get; }

        public TransformToLambert2008(
            ParcelId parcelId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"TransformToLambert2008-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
