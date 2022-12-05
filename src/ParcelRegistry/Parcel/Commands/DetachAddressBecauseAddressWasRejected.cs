namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class DetachAddressBecauseAddressWasRejected : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("4dcd8e86-eafc-494a-b50d-94160de973fc");

        public ParcelId ParcelId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public DetachAddressBecauseAddressWasRejected(
            ParcelId parcelId, 
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"DetachAddressBecauseAddressWasRejected-{ToString()}");

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
