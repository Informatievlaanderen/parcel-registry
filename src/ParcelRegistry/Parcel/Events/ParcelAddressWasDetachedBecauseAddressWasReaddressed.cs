namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Address)]
    [EventName(EventName)]
    [EventDescription("Het adres werd ontkoppeld van het perceel door heradressering.")]
    public sealed class ParcelAddressWasDetachedBecauseAddressWasReaddressed : IParcelEvent
    {
        public const string EventName = "ParcelAddressWasDetachedBecauseAddressWasReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Objectidentificator van het adres.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressWasDetachedBecauseAddressWasReaddressed(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            ParcelId = parcelId;
            CaPaKey = vbrCaPaKey;
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        [JsonConstructor]
        private ParcelAddressWasDetachedBecauseAddressWasReaddressed(
            Guid parcelId,
            string caPaKey,
            int addressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                new AddressPersistentLocalId(addressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            fields.Add(AddressPersistentLocalId.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
