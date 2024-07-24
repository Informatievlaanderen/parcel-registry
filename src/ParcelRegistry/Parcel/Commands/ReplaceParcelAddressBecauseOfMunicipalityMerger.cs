namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ReplaceParcelAddressBecauseOfMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("8c9fb5a8-9629-4798-b927-a89d07f7ae86");
        public ParcelId ParcelId { get; }
        public AddressPersistentLocalId NewAddressPersistentLocalId { get; }
        public AddressPersistentLocalId PreviousAddressPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public ReplaceParcelAddressBecauseOfMunicipalityMerger(
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
            => Deterministic.Create(Namespace, $"ReplaceParcelAddressBecauseOfMunicipalityMerger-{ToString()}");

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
