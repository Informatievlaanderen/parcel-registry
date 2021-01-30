namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("ParcelWasRegistered")]
    [EventDescription("Het perceel werd aangemaakt in het register met zijn persistente lokale identificator.")]
    public class ParcelWasRegistered : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string VbrCaPaKey { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasRegistered(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey)
        {
            ParcelId = parcelId;
            VbrCaPaKey = vbrCaPaKey;
        }

        [JsonConstructor]
        private ParcelWasRegistered(
            Guid parcelId,
            string vbrCaPaKey,
            ProvenanceData provenance)
            : this(new ParcelId(parcelId),
                new VbrCaPaKey(vbrCaPaKey)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
