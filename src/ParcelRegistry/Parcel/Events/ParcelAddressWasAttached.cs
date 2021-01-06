namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelAddressWasAttached")]
    [EventDescription("Er werd een adres gekoppeld aan het perceel.")]
    public class ParcelAddressWasAttached : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }
        
        [EventPropertyDescription("Interne GUID van het adres dat aan het perceel werd gekoppeld.")]
        public Guid AddressId { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressWasAttached(
            ParcelId parcelId,
            AddressId addressId)
        {
            ParcelId = parcelId;
            AddressId = addressId;
        }

        [JsonConstructor]
        private ParcelAddressWasAttached(
            Guid parcelId,
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new AddressId(addressId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
