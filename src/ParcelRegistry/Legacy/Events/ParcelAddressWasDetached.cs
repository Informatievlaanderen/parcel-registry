namespace ParcelRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("ParcelAddressWasDetached")]
    [EventDescription("Er werd een adres losgekoppeld van het perceel.")]
    public class ParcelAddressWasDetached : IMessage, IHasProvenance, ISetProvenance
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
