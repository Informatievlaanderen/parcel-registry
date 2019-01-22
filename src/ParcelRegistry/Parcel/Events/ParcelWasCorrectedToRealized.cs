namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    [EventName("ParcelWasCorrectedToRealized")]
    [EventDescription("Het perceel werd gerealiseerd via correctie.")]
    public class ParcelWasCorrectedToRealized : IHasProvenance, ISetProvenance
    {
        public Guid ParcelId { get; }
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasCorrectedToRealized(
            ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        [JsonConstructor]
        private ParcelWasCorrectedToRealized(
            Guid parcelId,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
