namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelAddressWasDetached")]
    [EventDescription("Er werd een adres losgekoppeld van het perceel.")]
    public class ParcelAddressWasDetached : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }
        
        [EventPropertyDescription("Interne GUID van het adres dat van het perceel werd losgekoppeld.")]
        public Guid AddressId { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelAddressWasDetached(
            ParcelId parcelId,
            AddressId addressId)
        {
            ParcelId = parcelId;
            AddressId = addressId;
        }

        [JsonConstructor]
        private ParcelAddressWasDetached(
            Guid parcelId,
            Guid addressId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                new AddressId(addressId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
