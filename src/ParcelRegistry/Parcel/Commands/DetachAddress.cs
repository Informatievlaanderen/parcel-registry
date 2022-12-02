namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class DetachAddress : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("a855293c-435a-438d-a242-4e927a40b48e");

        public ParcelId ParcelId { get; }

        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public DetachAddress(
            ParcelId parcelId,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"DetachAddress-{ToString()}");

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
