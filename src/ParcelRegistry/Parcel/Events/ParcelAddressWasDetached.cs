namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelAddressWasDetached")]
    [EventDescription("Aan het perceel werd een adres ontkoppeld.")]
    public class ParcelAddressWasDetached : IHasProvenance, ISetProvenance
    {
        public Guid ParcelId { get; }
        public Guid AddressId { get; }
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
