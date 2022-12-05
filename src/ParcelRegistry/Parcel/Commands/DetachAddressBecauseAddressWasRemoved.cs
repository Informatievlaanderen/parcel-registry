namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class DetachAddressBecauseAddressWasRemoved : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("f055da33-0c79-4616-a1e5-c1e21c8ab418");

        public ParcelId ParcelId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public DetachAddressBecauseAddressWasRemoved(
            ParcelId parcelId,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"DetachAddressBecauseAddressWasRemoved-{ToString()}");

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
