namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using ParcelRegistry.Parcel;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("Het perceel met status gehistoreerd werd gecorrigeerd naar status gerealiseerd.")]
    public sealed class ParcelWasCorrectedFromRetiredToRealized : IParcelEvent
    {
        public const string EventName = "ParcelWasCorrectedFromRetiredToRealized"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de perceelgeometrie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasCorrectedFromRetiredToRealized(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            ParcelId = parcelId;
            CaPaKey = vbrCaPaKey.ToString();
            ExtendedWkbGeometry = extendedWkbGeometry.ToString();
        }

        [JsonConstructor]
        private ParcelWasCorrectedFromRetiredToRealized(
            Guid parcelId,
            string caPaKey,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this (
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                new ExtendedWkbGeometry(extendedWkbGeometry))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            fields.Add(ExtendedWkbGeometry);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
