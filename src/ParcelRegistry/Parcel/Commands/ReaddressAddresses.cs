namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ReaddressAddresses : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("646d3ef7-6cbc-4b33-b75f-e5d72e48c356");
        public ParcelId ParcelId { get; }
        public IReadOnlyList<ReaddressData> Addresses { get; }
        public Provenance Provenance { get; }

        public ReaddressAddresses(
            ParcelId parcelId,
            IEnumerable<ReaddressData> addresses,
            Provenance provenance)
        {
            ParcelId = parcelId;
            Addresses = addresses.ToList();
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ReaddressAddresses-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;

            foreach (var address in Addresses)
            {
                yield return address.SourceAddressPersistentLocalId;
                yield return address.DestinationAddressPersistentLocalId;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }

    public class ReaddressData
    {
        public AddressPersistentLocalId SourceAddressPersistentLocalId { get; }
        public AddressPersistentLocalId DestinationAddressPersistentLocalId { get; }

        public ReaddressData(
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
        }
    }
}
