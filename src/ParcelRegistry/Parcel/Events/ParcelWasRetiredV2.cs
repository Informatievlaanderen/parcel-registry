namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("Het perceel werd gehistoreerd.")]
    public sealed class ParcelWasRetiredV2 : IParcelEvent
    {
        public const string EventName = "ParcelWasRetiredV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasRetiredV2(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey)
        {
            ParcelId = parcelId;
            CaPaKey = vbrCaPaKey;
        }

        [JsonConstructor]
        private ParcelWasRetiredV2(
            Guid parcelId,
            string caPaKey,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey)
                )
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
