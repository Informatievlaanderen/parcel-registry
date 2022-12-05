namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class DetachAddressBecauseAddressWasRetired : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("af4ebeb0-2527-4c57-9f3b-b21dad889ec4");

        public ParcelId ParcelId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public DetachAddressBecauseAddressWasRetired(
            ParcelId parcelId,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"DetachAddressBecauseAddressWasRetired-{ToString()}");

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
