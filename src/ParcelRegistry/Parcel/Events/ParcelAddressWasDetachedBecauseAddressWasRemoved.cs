namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, Tag.Address)]
    [EventName(EventName)]
    [EventDescription("Het adres werd ontkoppeld van het perceel door verwijdering adres.")]
    public sealed class ParcelAddressWasDetachedBecauseAddressWasRemoved : IParcelEvent
    {
        public const string EventName = "ParcelAddressWasDetachedBecauseAddressWasRemoved"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Objectidentificator van het adres dat ontkoppeld is van de perceeleenheid.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressWasDetachedBecauseAddressWasRemoved(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            CaPaKey = vbrCaPaKey;
        }

        [JsonConstructor]
        private ParcelAddressWasDetachedBecauseAddressWasRemoved(
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
