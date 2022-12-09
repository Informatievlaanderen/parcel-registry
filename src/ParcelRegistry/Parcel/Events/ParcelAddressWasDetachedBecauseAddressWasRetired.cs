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
    [EventDescription("Het adres werd ontkoppeld van het perceel door historering adres.")]
    public sealed class ParcelAddressWasDetachedBecauseAddressWasRetired : IParcelEvent
    {
        public const string EventName = "ParcelAddressWasDetachedBecauseAddressWasRetired"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("Objectidentificator van het adres dat ontkoppeld is van de perceeleenheid.")]
        public int AddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressWasDetachedBecauseAddressWasRetired(
            ParcelId parcelId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        [JsonConstructor]
        private ParcelAddressWasDetachedBecauseAddressWasRetired(
            Guid parcelId,
            int addressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new AddressPersistentLocalId(addressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(AddressPersistentLocalId.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
