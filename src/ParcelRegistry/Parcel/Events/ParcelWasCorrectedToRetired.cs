namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelWasCorrectedToRetired")]
    [EventDescription("Het perceel werd gehistoreerd via correctie.")]
    public class ParcelWasCorrectedToRetired : IHasProvenance, ISetProvenance
    {
        public Guid ParcelId { get; }
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasCorrectedToRetired(
            ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        [JsonConstructor]
        private ParcelWasCorrectedToRetired(
            Guid parcelId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
