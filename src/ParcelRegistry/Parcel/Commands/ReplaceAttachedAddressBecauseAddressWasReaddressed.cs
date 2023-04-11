namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class ReplaceAttachedAddressBecauseAddressWasReaddressed : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("35b8a5cf-df02-4350-b7f7-f9ebc506dfcb");

        public ParcelId ParcelId { get; }
        public AddressPersistentLocalId NewAddressPersistentLocalId { get; }
        public AddressPersistentLocalId PreviousAddressPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public ReplaceAttachedAddressBecauseAddressWasReaddressed(
            ParcelId parcelId,
            AddressPersistentLocalId newAddressPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId,
            Provenance provenance)
        {
            ParcelId = parcelId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
            PreviousAddressPersistentLocalId = previousAddressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ReplaceAttachedAddressBecauseAddressWasReaddressed-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
            yield return NewAddressPersistentLocalId;
            yield return PreviousAddressPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
