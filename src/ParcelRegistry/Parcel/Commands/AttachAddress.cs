namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class AttachAddress : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("d24d3899-5f76-43b4-9bdb-6f68335e327b");

        public ParcelId ParcelId { get; }

        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public AttachAddress(
            ParcelId parcelId,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"AttachAddress-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
            yield return AddressPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
