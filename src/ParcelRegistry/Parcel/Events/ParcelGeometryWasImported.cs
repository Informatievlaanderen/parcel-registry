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
    [EventDescription("De geometrie van het perceel is geimporteerd.")]
    public sealed class ParcelGeometryWasImported : IParcelEvent
    {
        public const string EventName = "ParcelGeometryWasImported"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("De geometrie van het perceel.")]
        public string Geometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelGeometryWasImported(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            ExtendedWkbGeometry geometry)
        {
            ParcelId = parcelId;
            CaPaKey = vbrCaPaKey.ToString();
            Geometry = geometry.ToString();
        }

        [JsonConstructor]
        private ParcelGeometryWasImported(
            Guid parcelId,
            string caPaKey,
            string geometry,
            ProvenanceData provenance)
            : this (
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                new ExtendedWkbGeometry(geometry)) // TODO: whats difference between extensions func geometry.ToExtendedWkbGeometry()) and new ExtendedWkbGeometry?
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            fields.Add(Geometry);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
