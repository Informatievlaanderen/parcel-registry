namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelWasRemoved")]
    [EventDescription("Het perceel werd verwijderd.")]
    public class ParcelWasRemoved : IHasProvenance, ISetProvenance
    {
        public Guid ParcelId { get; }
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasRemoved(
            ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        [JsonConstructor]
        private ParcelWasRemoved(
            Guid parcelId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
