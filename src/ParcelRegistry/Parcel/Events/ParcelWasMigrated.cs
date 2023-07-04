namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, Tag.Migration)]
    [EventName(EventName)]
    [EventDescription("Het perceel werd gemigreerd.")]
    public sealed class ParcelWasMigrated : IParcelEvent
    {
        public const string EventName = "ParcelWasMigrated"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het gemigreerde perceel.")]
        public Guid OldParcelId { get; }

        [EventPropertyDescription("Interne GUID van het nieuwe perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("De status van het perceel. Mogelijkheden: Realized en Retired.")]
        public string ParcelStatus { get; }

        [EventPropertyDescription("False wanneer het perceel niet werd verwijderd. True wanneer het perceel werd verwijderd.")]
        public bool IsRemoved { get; }

        [EventPropertyDescription("Objectidentificatoren van adressen die gekoppeld zijn aan de perceeleenheid.")]
        public List<int> AddressPersistentLocalIds { get; }

        [EventPropertyDescription("De geometrie van het perceel.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasMigrated(
            Legacy.ParcelId oldParcelId,
            ParcelId newParcelId,
            VbrCaPaKey caPaKey,
            ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            OldParcelId = oldParcelId;
            ParcelId = newParcelId;
            CaPaKey = caPaKey;
            ParcelStatus = parcelStatus;
            IsRemoved = isRemoved;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(x => (int)x).ToList();
            ExtendedWkbGeometry = extendedWkbGeometry;
        }

        [JsonConstructor]
        private ParcelWasMigrated(
            Guid oldParcelId,
            Guid parcelId,
            string caPaKey,
            string parcelStatus,
            bool isRemoved,
            IEnumerable<int> addressPersistentLocalIds,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new Legacy.ParcelId(oldParcelId),
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                ParcelRegistry.Parcel.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                addressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                new ExtendedWkbGeometry(extendedWkbGeometry))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(OldParcelId.ToString("D"));
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            fields.Add(ParcelStatus);
            fields.Add(IsRemoved.ToString());
            fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            fields.Add(ExtendedWkbGeometry);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
