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
    [EventDescription("Het adres werd gekoppeld aan het perceel.")] //TODO-rik event description
    public sealed class ParcelAddressWasReplacedBecauseOfMunicipalityMerger : IParcelEvent
    {
        public const string EventName = "ParcelAddressWasReplacedBecauseOfMunicipalityMerger"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("CaPaKey (= objectidentificator) van het perceel, waarbij forward slashes vervangen zijn door koppeltekens i.f.v. gebruik in URI's.")]
        public string CaPaKey { get; }

        [EventPropertyDescription("Objectidentificator van het nieuwe adres.")]
        public int NewAddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het vorige adres.")]
        public int PreviousAddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressWasReplacedBecauseOfMunicipalityMerger(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey,
            AddressPersistentLocalId newAddressPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId)
        {
            ParcelId = parcelId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
            PreviousAddressPersistentLocalId = previousAddressPersistentLocalId;
            CaPaKey = vbrCaPaKey;
        }

        [JsonConstructor]
        private ParcelAddressWasReplacedBecauseOfMunicipalityMerger(
            Guid parcelId,
            string caPaKey,
            int newAddressPersistentLocalId,
            int previousAddressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                new AddressPersistentLocalId(newAddressPersistentLocalId),
                new AddressPersistentLocalId(previousAddressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(CaPaKey);
            fields.Add(NewAddressPersistentLocalId.ToString());
            fields.Add(PreviousAddressPersistentLocalId.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
