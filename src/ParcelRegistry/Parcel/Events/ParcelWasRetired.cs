namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelWasRetired")]
    [EventDescription("Het perceel werd gehistoreerd.")]
    public class ParcelWasRetired : IHasProvenance, ISetProvenance
    {
        public Guid ParcelId { get; }
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasRetired(
            ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        [JsonConstructor]
        private ParcelWasRetired(
            Guid parcelId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
