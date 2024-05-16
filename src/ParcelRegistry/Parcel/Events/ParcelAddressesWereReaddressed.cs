namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Address)]
    [EventName(EventName)]
    [EventDescription("De adresherkoppelingen op het perceel door heradressering.")]
    public sealed class ParcelAddressesWereReaddressed : IParcelEvent
    {
        public const string EventName = "ParcelAddressesWereReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Objectidentificatoren van nieuw gekoppelde adressen.")]
        public IEnumerable<int> AttachedAddressPersistentLocalIds { get; }

        [EventPropertyDescription("Objectidentificatoren van ontkoppelde adressen.")]
        public IEnumerable<int> DetachedAddressPersistentLocalIds { get; }

        [EventPropertyDescription("De geheradresseerde adressen uit het Adressenregister.")]
        public IEnumerable<AddressRegistryReaddress> AddressRegistryReaddresses { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressesWereReaddressed(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            IEnumerable<AddressPersistentLocalId> attachedAddressPersistentLocalIds,
            IEnumerable<AddressPersistentLocalId> detachedAddressPersistentLocalIds,
            IEnumerable<AddressRegistryReaddress> addressRegistryReaddresses)
        {
            ParcelId = parcelId;
            CaPaKey = vbrCaPaKey;
            AttachedAddressPersistentLocalIds = attachedAddressPersistentLocalIds.Select(x => (int)x).ToList();
            DetachedAddressPersistentLocalIds = detachedAddressPersistentLocalIds.Select(x => (int)x).ToList();
            AddressRegistryReaddresses = addressRegistryReaddresses;
        }

        [JsonConstructor]
        private ParcelAddressesWereReaddressed(
            Guid parcelId,
            string caPaKey,
            IEnumerable<int> attachedAddressPersistentLocalIds,
            IEnumerable<int> detachedAddressPersistentLocalIds,
            IEnumerable<AddressRegistryReaddress> addressRegistryReaddresses,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                attachedAddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                detachedAddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                addressRegistryReaddresses)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            fields.AddRange(AttachedAddressPersistentLocalIds.Select(addressPersistentLocalId => addressPersistentLocalId.ToString()));
            fields.AddRange(DetachedAddressPersistentLocalIds.Select(addressPersistentLocalId => addressPersistentLocalId.ToString()));

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }

    public sealed class AddressRegistryReaddress
    {
        [EventPropertyDescription("Objectidentificator van het bronadres.")]
        public int SourceAddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het doeladres.")]
        public int DestinationAddressPersistentLocalId { get; }

        public AddressRegistryReaddress(
            ReaddressData readdressData)
        {
            SourceAddressPersistentLocalId = readdressData.SourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = readdressData.DestinationAddressPersistentLocalId;
        }

        [JsonConstructor]
        private AddressRegistryReaddress(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
        }
    }
}
