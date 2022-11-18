namespace ParcelRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("ParcelWasMarkedAsMigrated")]
    [EventDescription("Het perceel werd gemarkeerd als gemigreerd.")]
    public class ParcelWasMarkedAsMigrated : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasMarkedAsMigrated(
            ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        [JsonConstructor]
        private ParcelWasMarkedAsMigrated(
            Guid parcelId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
