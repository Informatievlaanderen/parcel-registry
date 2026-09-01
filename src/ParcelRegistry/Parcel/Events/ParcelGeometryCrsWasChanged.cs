namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ParcelRegistry.Parcel;

    /// <summary>
    /// The geometry was re-expressed in another coordinate reference system without moving: the one-off
    /// transformation of the event store to Lambert 2008 (EPSG 3812), see ADR 0005. Mirrors the
    /// <c>ParcelGeometryCrsWasChanged</c> contract in GrAr.Contracts, which is why it restates the CaPaKey
    /// like <see cref="ParcelGeometryWasChanged"/> even though the transformation does not change it.
    /// </summary>
    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("Het coördinatenreferentiesysteem van de perceelgeometrie werd gewijzigd.")]
    public sealed class ParcelGeometryCrsWasChanged : IParcelEvent
    {
        public const string EventName = "ParcelGeometryCrsWasChanged"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de perceelgeometrie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelGeometryCrsWasChanged(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            ParcelId = parcelId;
            CaPaKey = vbrCaPaKey.ToString();
            ExtendedWkbGeometry = extendedWkbGeometry.ToString();
        }

        [JsonConstructor]
        private ParcelGeometryCrsWasChanged(
            Guid parcelId,
            string caPaKey,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
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
